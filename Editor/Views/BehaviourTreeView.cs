using System;
using System.Collections.Generic;
using System.Linq;

using MoshitinEncoded.AI.BehaviourTreeLib;
using MoshitinEncoded.Editor;
using MoshitinEncoded.Editor.AI.BehaviourTreeLib;
using MoshitinEncoded.Editor.GraphTools;

using UnityEditor;
using UnityEditor.Experimental.GraphView;

using UnityEngine;
using UnityEngine.UIElements;

using Node = MoshitinEncoded.AI.BehaviourTreeLib.Node;

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

        ClearGraph();

        _BlackboardView.PopulateView(_BehaviourTree.Blackboard, _BehaviourTree.name, typeof(BehaviourTreeParameter<>));

        CreateNodeViews();
        CreateEdges();

        LoadViewTransform();
    }

    public void ClearView()
    {
        _BehaviourTree = null;
        _SerializedTree = null;
        _RootNodeView = null;
        ClearGraph();
        _BlackboardView.ClearView();
    }

    private void ClearGraph()
    {
        graphViewChanged -= OnGraphViewChange;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChange;
    }

    public void UpdateNodeStates()
    {
        nodes.ForEach(node =>
        {
            (node as NodeView)?.UpdateState();
        });
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
    }

    internal Node CreateNode(Type behaviourType, string nodeTitle, Vector2 position)
    {
        var node = ScriptableObject.CreateInstance<Node>();

        AddNodeToAsset(node);
        SetupNode(node, behaviourType, nodeTitle, position);
        AddNodeToArray(node);
        CreateNodeView(node);

        return node;
    }

    private void SetupNode(Node node, Type behaviourType, string nodeTitle, Vector2 position)
    {
        Undo.RegisterCompleteObjectUndo(node, "Create Node (Behaviour Tree)");

        node.name = behaviourType.Name;
        node.hideFlags = HideFlags.HideInHierarchy;

        var serializedNode = new SerializedObject(node);
        serializedNode.FindProperty("_Title").stringValue = nodeTitle;
        serializedNode.FindProperty("_Guid").stringValue = GUID.Generate().ToString();
        serializedNode.FindProperty("_Position").vector2Value = position;
        serializedNode.ApplyModifiedProperties();

        var behaviour = ScriptableObject.CreateInstance(behaviourType) as NodeBehaviour;

        if (behaviour)
        {
            AddBehaviourToAsset(behaviour);
            SetupBehaviour(behaviour);
            AddBehaviourToNode(node, behaviour);
        }
    }

    private void SetupBehaviour(NodeBehaviour behaviour)
    {
        Undo.RegisterCompleteObjectUndo(behaviour, "Create Node (Behaviour Tree)");

        behaviour.name = behaviour.GetType().Name;
        behaviour.hideFlags = HideFlags.HideInHierarchy;
    }

    private void CreateNodeViews()
    {
        foreach (var node in _BehaviourTree.Nodes)
        {
            if (node != null)
            {
                CreateNodeView(node);
            }
        }
    }

    private void CreateEdges()
    {
        foreach (var node in _BehaviourTree.Nodes)
        {
            if (!node)
            {
                continue;
            }

            foreach (var child in node.Children)
            {
                NodeView parentView = GetNodeView(node);
                NodeView childView = GetNodeView(child);

                Edge edge = parentView.Output.ConnectTo(childView.Input);
                AddElement(edge);
            }
        }
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
        var nodeViews = elements.Where(element => element is NodeView);
        var selectionRect = CalculateRectToFitElements(nodeViews);

        return GraphSerializer.SerializeSelection(selectionRect, elements);
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

        ClearSelection();

        foreach (var childDict in selection.ChildsDict)
        {
            var node = childDict.Key;
            var behaviour = node.Behaviour;

            AddNodeToAsset(node);
            SetupPasteNode(operationName, selection.SelectionRect, node);

            AddBehaviourToAsset(behaviour);

            Undo.RegisterCompleteObjectUndo(behaviour, "Paste Node (BehaviourTree)");

            behaviour.name = behaviour.GetType().Name;
            behaviour.hideFlags = HideFlags.HideInHierarchy;

            var serializedBehaviour = new SerializedObject(behaviour);
            serializedBehaviour.FindProperty("_Node").objectReferenceValue = node;
            serializedBehaviour.ApplyModifiedProperties();

            AddNodeToArray(node);
            CreateNodeView(node);
        }

        foreach (var childDict in selection.ChildsDict)
        {
            var parentView = GetNodeView(childDict.Key);
            AddToSelection(parentView);

            foreach (var child in childDict.Value)
            {
                parentView.AddChild(child);
                var childView = GetNodeView(child);
                var edge = parentView.Output.ConnectTo(childView.Input);
                AddElement(edge);
                AddToSelection(edge);
            }
        }
    }

    private void SetupPasteNode(string operationName, Rect selectionRect, Node node)
    {
        Undo.RegisterCompleteObjectUndo(node, "Paste Node (BehaviourTree)");

        node.name = "Node";
        node.hideFlags = HideFlags.HideInHierarchy;

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
        serializedNode.FindProperty("_Children").ClearArray();
        serializedNode.ApplyModifiedProperties();
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
                nodeView.OnGraphElementMoved();
            });
        }

        return graphViewChange;
    }

    private void AddBehaviourToAsset(NodeBehaviour behaviour)
    {
        if (!Application.isPlaying)
        {
            AssetDatabase.AddObjectToAsset(behaviour, _BehaviourTree);
        }

        Undo.RegisterCreatedObjectUndo(behaviour, "Create Node (Behaviour Tree)");
    }

    private void AddNodeToAsset(Node node)
    {
        if (!Application.isPlaying)
        {
            AssetDatabase.AddObjectToAsset(node, _BehaviourTree);
        }

        Undo.RegisterCreatedObjectUndo(node, "Create Node (Behaviour Tree)");
    }

    private void AddNodeToArray(Node node)
    {
        _SerializedTree.Update();
        _SerializedTree.FindProperty("_Nodes").AddToArray(node);
        _SerializedTree.ApplyModifiedProperties();
    }

    private void AddBehaviourToNode(Node node, NodeBehaviour behaviour)
    {
        var serializedNode = new SerializedObject(node);
        serializedNode.FindProperty("_Behaviour").objectReferenceValue = behaviour;
        serializedNode.ApplyModifiedProperties();

        var serializedBehaviour = new SerializedObject(behaviour);
        serializedBehaviour.FindProperty("_Node").objectReferenceValue = node;
        serializedBehaviour.ApplyModifiedProperties();
    }

    private void DeleteNode(Node node)
    {
        _SerializedTree.Update();

        var nodesProperty = _SerializedTree.FindProperty("_Nodes");
        var nodeRemoved = nodesProperty.RemoveFromArray(node);

        if (nodeRemoved)
        {
            _SerializedTree.ApplyModifiedProperties();

            if (node.Behaviour)
            {
                var behaviour = node.Behaviour;
                var serializedNode = new SerializedObject(node);
                serializedNode.FindProperty("_Behaviour").objectReferenceValue = null;
                serializedNode.ApplyModifiedProperties();
                
                Undo.DestroyObjectImmediate(behaviour);
            }

            Undo.DestroyObjectImmediate(node);
        }
    }

    private void CreateNodeView(Node node)
    {
        NodeView nodeView = null;
        switch (node.Behaviour)
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
            case TaskNode:
                nodeView = new TaskNodeView(node, this);
                break;
            case null:
                nodeView = new MissingNodeView(node, this);
                break;
        }

        if (nodeView == null)
        {
            return;
        }

        AddElement(nodeView);
    }
}