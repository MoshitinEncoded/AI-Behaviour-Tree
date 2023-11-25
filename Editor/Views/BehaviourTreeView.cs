using MoshitinEncoded.AI.BehaviourTreeLib;
using Node = MoshitinEncoded.AI.BehaviourTreeLib.Node;
using MoshitinEncoded.Editor;
using MoshitinEncoded.Editor.AI.BehaviourTreeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using MoshitinEncoded.Editor.GraphTools;

public partial class BehaviourTreeView : GraphView
{
    public new class UxmlFactory : UxmlFactory<BehaviourTreeView, UxmlTraits> { }

    private BehaviourTree _BehaviourTree;
    private SerializedObject _SerializedTree;
    private NodeSearchWindow _SearchWindow;
    private EditorWindow _Window;
    private BlackboardView _BlackboardView;
    private Vector2 _MousePosition;
    private RootNodeView _RootNodeView;

    public bool IsPopulated => _BehaviourTree != null;

    public BehaviourTreeView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>
        (
            "Packages/com.moshitin-encoded.ai.behaviourtree/Editor/Styles/BehaviourTreeEditor.uss"
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

    public void PopulateView(BehaviourTree tree)
    {
        if (tree == null)
        {
            return;
        }

        if (_BehaviourTree != tree)
        {
            _BehaviourTree = tree;
            _SerializedTree = new SerializedObject(tree);
        }

        graphViewChanged -= OnGraphViewChange;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChange;

        _BlackboardView.PopulateView(_BehaviourTree.Blackboard, _BehaviourTree.name, typeof(BehaviourTreeParameter<>));

        // Creates node views
        foreach (var node in _BehaviourTree.Nodes)
        {
            CreateNodeView(node);
        }

        // Creates node edges
        foreach (var node in _BehaviourTree.Nodes)
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
        //_RootNodeView?.UpdateState();
        nodes.ForEach(node =>
        {
            (node as NodeView).UpdateState();
        });
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.Where(endPort =>
        endPort.direction != startPort.direction &&
        endPort.node != startPort.node).ToList();
    }

    internal Node CreateNode(Type nodeType, string nodeTitle, Vector2 position)
    {
        var node = _BehaviourTree.CreateNode(nodeType);

        if (node == null)
        {
            return null;
        }

        AddNodeToProject(node);

        var serializedNode = new SerializedObject(node);
        serializedNode.FindProperty("_Guid").stringValue = GUID.Generate().ToString();
        serializedNode.FindProperty("_Position").vector2Value = position;
        serializedNode.FindProperty("_Title").stringValue = nodeTitle;
        serializedNode.ApplyModifiedPropertiesWithoutUndo();

        AddNodeToAsset(node);

        return node;
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        _MousePosition = contentViewContainer.WorldToLocal(evt.mousePosition);
    }

    internal NodeView GetNodeView(Node node)
    {
        string nodeGuid = new SerializedObject(node).FindProperty("_Guid").stringValue;
        return GetElementByGuid(nodeGuid) as NodeView;
    }

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
        var selection = GraphSerializer.UnserializeSelection(data);
        var topParents = GetTopParents(selection.ChildsDict);

        ClearSelection();

        foreach (var parent in topParents)
        {
            PasteNodeHierarchy(
                operationName: operationName,
                selectionRect: selection.SelectionRect,
                childsDict: selection.ChildsDict,
                node: parent
            );
        }
    }

    private NodeView PasteNodeHierarchy(string operationName, Rect selectionRect, Dictionary<Node, List<Node>> childsDict, Node node)
    {
        var isParentNode = node is IParentNode;
        var childNodes = childsDict[node];

        AddNodeToProject(node);

        // Set Guid and Position
        var serializedNode = new SerializedObject(node);
        var positionProperty = serializedNode.FindProperty("_Position");

        if (operationName == "Duplicate")
        {
            positionProperty.vector2Value += new Vector2(10, 10);
        }
        else
        {
            positionProperty.vector2Value = _MousePosition + positionProperty.vector2Value - selectionRect.center;
        }

        serializedNode.FindProperty("_Guid").stringValue = GUID.Generate().ToString();
        serializedNode.ApplyModifiedProperties();

        AddNodeToAsset(node);

        // Paste and get child nodes
        var nodeView = GetNodeView(node);
        var childNodeViews = new List<NodeView>();
        foreach (var childNode in childNodes)
        {
            childNodeViews.Add(PasteNodeHierarchy(operationName, selectionRect, childsDict, childNode));
        }

        Undo.RecordObject(node, "Create Node (Behaviour Tree)");
        node.name = node.GetType().Name;

        if (isParentNode)
        {
            var parentNode = node as IParentNode;
            parentNode.ClearChildren();
            foreach (var childNode in childNodes)
            {
                parentNode.AddChild(childNode);
            }

            foreach (var childNodeView in childNodeViews)
            {
                var edge = nodeView.Output.ConnectTo(childNodeView.Input);
                AddElement(edge);
                AddToSelection(edge);
            }
        }

        AddToSelection(nodeView);

        return nodeView;
    }

    private IEnumerable<Node> GetTopParents(Dictionary<Node, List<Node>> nodeChildsDict)
    {
        var topParents = nodeChildsDict.Keys.ToList();
        foreach (var nodeChildsPair in nodeChildsDict)
        {
            foreach (var childNode in nodeChildsPair.Value)
            {
                topParents.Remove(childNode);
            }
        }

        return topParents;
    }

    private void OnUndoRedo()
    {
        PopulateView(_BehaviourTree);
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
        if (_BehaviourTree == null)
        {
            return;
        }

        viewTransform.position = _SerializedTree.FindProperty("_GraphPosition").vector2Value;
        viewTransform.scale = _SerializedTree.FindProperty("_GraphScale").vector2Value;
    }

    private void OnViewTransformChanged(GraphView graphView)
    {
        if (_BehaviourTree == null)
        {
            return;
        }

        _SerializedTree.Update();
        _SerializedTree.FindProperty("_GraphPosition").vector2Value = viewTransform.position;
        _SerializedTree.FindProperty("_GraphScale").vector2Value = viewTransform.scale;
        _SerializedTree.ApplyModifiedPropertiesWithoutUndo();
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
                _BlackboardView.RemoveParameter(blackboardField);
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
                nodeView.OnMoved();
            });
        }

        return graphViewChange;
    }

    private void AddNodeToProject(Node node)
    {
        if (!Application.isPlaying)
        {
            AssetDatabase.AddObjectToAsset(node, _BehaviourTree);
        }

        Undo.RegisterCreatedObjectUndo(node, "Create Node (Behaviour Tree)");
    }

    private void AddNodeToAsset(Node node)
    {
        _SerializedTree.Update();
        var nodesProperty = _SerializedTree.FindProperty("_Nodes");
        nodesProperty.AddToObjectArray(node);
        _SerializedTree.ApplyModifiedProperties();

        CreateNodeView(node);
    }

    private void DeleteNode(Node node)
    {
        if (node is RootNode)
        {
            return;
        }

        _SerializedTree.Update();

        var nodesProperty = _SerializedTree.FindProperty("_Nodes");
        var nodeRemoved = nodesProperty.RemoveFromObjectArray(node);

        if (nodeRemoved)
        {
            _SerializedTree.ApplyModifiedProperties();
            Undo.DestroyObjectImmediate(node);
        }
    }

    private void CreateNodeView(Node node)
    {
        NodeView nodeView;
        switch (node)
        {
            case RootNode:
                _RootNodeView = new RootNodeView(node, this);
                nodeView = _RootNodeView;
                break;
            case DecoratorNode:
                nodeView = new DecoratorNodeView(node, this);
                break;
            case CompositeNode:
                nodeView = new CompositeNodeView(node, this);
                break;
            case ConditionNode:
                nodeView = new ConditionNodeView(node, this);
                break;
            case ActionNode:
                nodeView = new ActionNodeView(node, this);
                break;
            default:
                nodeView = new NodeView(node, this);
                break;
        }

        AddElement(nodeView);
    }
}