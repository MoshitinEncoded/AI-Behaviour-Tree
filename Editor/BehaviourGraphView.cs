using MoshitinEncoded.BehaviourTree;
using Node = MoshitinEncoded.BehaviourTree.Node;
using MoshitinEncoded.Editor;
using MoshitinEncoded.Editor.BehaviourTree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public partial class BehaviourGraphView : GraphView
{
    public new class UxmlFactory : UxmlFactory<BehaviourGraphView, UxmlTraits> { }

    private BehaviourTreeController _GraphAsset;
    private SerializedObject _SerializedGraph;
    private NodeSearchWindow _SearchWindow;
    private EditorWindow _Window;
    private BlackboardView _BlackboardView;
    private Vector2 _MousePosition;

    public bool IsPopulated => _GraphAsset != null;

    public BehaviourGraphView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>
        (
            "Packages/com.moshitin-encoded.behaviourgraph/Editor/BehaviourGraphEditor.uss"
        );

        styleSheets.Add(styleSheet);

        serializeGraphElements += OnSerializeElements;
        unserializeAndPaste += OnPasteElements;

        viewTransformChanged += OnViewTransformChanged;

        RegisterCallback<MouseMoveEvent>(OnMouseMove);

        Undo.undoRedoPerformed += OnUndoRedo;
    }

    public void Init(EditorWindow editorWindow)
    {
        _Window = editorWindow;
        AddSearchWindow();
        CreateBlackboard();
    }

    public void PopulateView(BehaviourTreeController tree)
    {
        if (tree == null)
        {
            return;
        }

        if (_GraphAsset != tree)
        {
            _GraphAsset = tree;
            _SerializedGraph = new SerializedObject(tree);
        }

        graphViewChanged -= OnGraphViewChange;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChange;

        _BlackboardView.PopulateView(_GraphAsset, _SerializedGraph);

        if (_GraphAsset.RootNode == null)
        {
            _SerializedGraph.Update();

            SerializedProperty rootNodeProperty = _SerializedGraph.FindProperty("_RootNode");
            rootNodeProperty.objectReferenceValue = CreateNode(typeof(RootNode), "Root", Vector2.zero, false) as RootNode;

            _SerializedGraph.ApplyModifiedPropertiesWithoutUndo();
        }

        // Creates node views
        foreach (var node in _GraphAsset.Nodes)
        {
            CreateNodeView(node);
        }

        // Creates node edges
        foreach (var node in _GraphAsset.Nodes)
        {
            if (node is IParentNode parentNode)
            {
                var children = parentNode.GetChildren();
                children.ForEach(childNode =>
                {
                    NodeView parentView = GetNodeView(node);
                    NodeView childView = GetNodeView(childNode);

                    Edge edge = parentView.Output.ConnectTo(childView.Input);
                    AddElement(edge);
                });
            }
        }

        LoadViewTransform();
    }

    public void UpdateNodeStates()
    {
        nodes.ForEach(node =>
        {
            var nodeView = node as NodeView;
            nodeView.UpdateState();
        });
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.Where(endPort =>
        endPort.direction != startPort.direction &&
        endPort.node != startPort.node).ToList();
    }

    internal Node CreateNode(Type nodeType, string nodeTitle, Vector2 position, bool registerUndo = true)
    {
        // Creates the node
        var node = ScriptableObject.CreateInstance(nodeType) as Node;

        if (node == null)
        {
            return null;
        }

        node.Title = nodeTitle;
        node.name = nodeType.Name;
        node.guid = GUID.Generate().ToString();
        node.position = position;
        //node.hideFlags = HideFlags.HideInHierarchy;

        AddNode(node, registerUndo);

        return node;
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        _MousePosition = contentViewContainer.WorldToLocal(evt.mousePosition);
    }

    private NodeView GetNodeView(Node node) => GetElementByGuid(node.guid) as NodeView;

    /// <summary>
    /// Called on Copy/Cut/Duplicate operations.
    /// </summary>
    /// <param name="elements"></param>
    /// <returns> The selection serialized as JSON. </returns>
    private string OnSerializeElements(IEnumerable<GraphElement> elements)
    {
        elements = FilterOutRootNode(elements);
        var nodeViews = elements.Where(element => element is NodeView);
        var selectionRect = CalculateRectToFitElements(nodeViews);
        
        return GraphSerializer.SerializeSelection(selectionRect, elements);
    }

    private IEnumerable<GraphElement> FilterOutRootNode(IEnumerable<GraphElement> elements)
    {
        var newElements = new List<GraphElement>();
        foreach (var element in elements)
        {
            if (element is NodeView nodeView && nodeView.Node is not RootNode)
            {
                newElements.Add(nodeView);
            }
            else if (element is Edge edge && 
                elements.Contains(edge.input.node) && 
                elements.Contains(edge.output.node) && 
                (edge.output.node as NodeView).Node is not RootNode)
            {
                newElements.Add(edge);
            }
        }

        return newElements;
    }

    private static Rect CalculateRectToFitElements(IEnumerable<GraphElement> elements)
    {
        Rect rectToFit = new();
        bool reachedFirstElement = false;
        foreach (var element in elements)
        {
            if (!reachedFirstElement)
            {
                rectToFit = element.GetPosition();
                reachedFirstElement = true;
            }
            else
            {
                rectToFit = RectUtils.Encompass(rectToFit, element.GetPosition());
            }
        }

        return rectToFit;
    }

    private void OnPasteElements(string operationName, string data)
    {
        var graphSelection = GraphSerializer.UnserializeSelection(data);
        var topNodes = GetTopParents(graphSelection.Nodes);

        ClearSelection();

        topNodes.ForEach(topNode =>PasteNodeHierarchy(
            operationName: operationName,
            selectionRect: graphSelection.SelectionRect,
            node: topNode));
    }

    private NodeView PasteNodeHierarchy(string operationName, Rect selectionRect, Node node)
    {
        if (operationName == "Duplicate")
        {
            node.position += new Vector2(10, 10);
        }
        else
        {
            node.position = _MousePosition + node.position - selectionRect.center;
        }

        AddNode(node, registerUndo: true);

        var nodeView = GetNodeView(node);
        AddToSelection(nodeView);

        if (node is IParentNode parentNode)
        {
            parentNode.GetChildren().ForEach(childNode => 
            {
                var childNodeView = PasteNodeHierarchy(operationName, selectionRect, childNode);
                var edge = nodeView.Output.ConnectTo(childNodeView.Input);
                AddElement(edge);
            });
        }

        return nodeView;
    }

    private List<Node> GetTopParents(IEnumerable<Node> nodes)
    {
        var topParents = new List<Node>(nodes);
        foreach (var node in nodes)
        {
            if (node is IParentNode parentNode)
            {
                parentNode.GetChildren().ForEach(child => topParents.Remove(child));
            }
        }

        return topParents;
    }

    private void OnUndoRedo()
    {
        PopulateView(_GraphAsset);
    }

    private void AddSearchWindow()
    {
        _SearchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _SearchWindow.Init(_Window, this);
        nodeCreationRequest += context =>
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _SearchWindow);
    }

    private void CreateBlackboard()
    {
        _BlackboardView = new(this);
        Add(_BlackboardView);
    }

    

    private void LoadViewTransform()
    {
        if (_GraphAsset == null)
        {
            return;
        }

        viewTransform.position = _SerializedGraph.FindProperty("_GraphPosition").vector2Value;
        viewTransform.scale = _SerializedGraph.FindProperty("_GraphScale").vector2Value;
    }

    private void OnViewTransformChanged(GraphView graphView)
    {
        if (_GraphAsset == null)
        {
            return;
        }

        _SerializedGraph.Update();
        _SerializedGraph.FindProperty("_GraphPosition").vector2Value = viewTransform.position;
        _SerializedGraph.FindProperty("_GraphScale").vector2Value = viewTransform.scale;
        _SerializedGraph.ApplyModifiedPropertiesWithoutUndo();
    }

    private GraphViewChange OnGraphViewChange(GraphViewChange graphViewChange)
    {
        graphViewChange.elementsToRemove?.ForEach(element =>
        {
            if (element is NodeView nodeView)
            {
                DeleteNode(nodeView.Node);
            }
            else if (element is Edge edge)
            {
                NodeView parentView = edge.output.node as NodeView;
                NodeView childView = edge.input.node as NodeView;
                parentView.RemoveChild(childView.Node);
            }
            else if (element is BlackboardField blackboardField)
            {
                _BlackboardView.DeleteProperty(blackboardField);
            }
        });

        graphViewChange.edgesToCreate?.ForEach(edge =>
        {
            NodeView parentView = edge.output.node as NodeView;
            NodeView childView = edge.input.node as NodeView;
            parentView.AddChild(childView.Node);
        });

        if (graphViewChange.movedElements != null)
        {
            nodes.ForEach(node =>
            {
                var nodeView = node as NodeView;
                nodeView.SortChildren();
            });
        }

        return graphViewChange;
    }

    private void AddNode(Node node, bool registerUndo)
    {
        // Adds the node to the project
        if (!Application.isPlaying)
        {
            AssetDatabase.AddObjectToAsset(node, _GraphAsset);
        }

        if (registerUndo)
        {
            Undo.RegisterCreatedObjectUndo(node, "Create Node (Behaviour Tree)");
        }

        // Adds the node to the list
        _SerializedGraph.Update();
        
        var nodesProperty = _SerializedGraph.FindProperty("_Nodes");
        nodesProperty.AddToObjectArray(node);

        if (registerUndo)
        {
            _SerializedGraph.ApplyModifiedProperties();
        }
        else
        {
            _SerializedGraph.ApplyModifiedPropertiesWithoutUndo();
        }

        CreateNodeView(node);
    }

    private void DeleteNode(Node node)
    {
        if (node is RootNode)
        {
            return;
        }

        _SerializedGraph.Update();

        var nodesProperty = _SerializedGraph.FindProperty("_Nodes");
        var nodeRemoved = nodesProperty.RemoveFromObjectArray(node);

        if (nodeRemoved)
        {
            _SerializedGraph.ApplyModifiedProperties();
            Undo.DestroyObjectImmediate(node);
        }
    }

    private void CreateNodeView(Node node)
    {
        var nodeView = new NodeView(node);
        AddElement(nodeView);
    }
}