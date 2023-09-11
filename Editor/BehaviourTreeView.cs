using MoshitinEncoded.AI;
using MoshitinEncoded.AI.Editor;
using MoshitinEncoded.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class BehaviourTreeView : GraphView
{
    public new class UxmlFactory : UxmlFactory<BehaviourTreeView, UxmlTraits> { }

    private BehaviourTree _tree;
    private SerializedObject _serializedTree;
    private NodeSearchWindow _searchWindow;
    private EditorWindow _window;
    private BlackboardView _blackboardView;
    private BlackboardSection _propertiesSection;

    public BehaviourTreeView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.moshitin-encoded.tree-ai/Editor/BehaviourTreeEditor.uss");
        styleSheets.Add(styleSheet);

        Undo.undoRedoPerformed += OnUndoRedo;
        viewTransformChanged += SaveViewTransform;
    }

    private void OnUndoRedo()
    {
        PopulateView(_tree);
    }

    public void Init(EditorWindow editorWindow)
    {
        _window = editorWindow;
        AddSearchWindow();
        CreateBlackboard();
    }

    public bool IsPopulated() =>
        _tree != null;

    private void AddSearchWindow()
    {
        _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _searchWindow.Init(_window, this);
        nodeCreationRequest += context =>
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
    }

    private void CreateBlackboard()
    {
        _blackboardView = new(this);
        Add(_blackboardView);
    }

    public NodeView FindNodeView(MoshitinEncoded.AI.Node node) =>
        GetElementByGuid(node.guid) as NodeView;

    internal void PopulateView(BehaviourTree tree)
    {
        if (tree == null)
        {
            return;
        }

        if (_tree != tree)
        {
            _tree = tree;
            _serializedTree = new SerializedObject(tree);
        }

        graphViewChanged -= OnGraphViewChange;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChange;

        _blackboardView.PopulateView(_tree, _serializedTree);

        if (_tree.rootNode == null)
        {
            _serializedTree.Update();

            SerializedProperty rootNodeProperty = _serializedTree.FindProperty("rootNode");
            rootNodeProperty.objectReferenceValue = CreateNode(typeof(RootNode), "Root", Vector2.zero, false) as RootNode;

            _serializedTree.ApplyModifiedPropertiesWithoutUndo();
        }

        // Creates node views
        _tree.Nodes.ForEach(CreateNodeView);

        // Creates node edges
        _tree.Nodes.ForEach(parentNode =>
        {
            var children = _tree.GetChildren(parentNode);
            children.ForEach(childNode =>
            {
                NodeView parentView = FindNodeView(parentNode);
                NodeView childView = FindNodeView(childNode);

                Edge edge = parentView.Output.ConnectTo(childView.Input);
                AddElement(edge);
            });
        });

        LoadViewTransform();
    }

    private void LoadViewTransform()
    {
        if (_tree == null)
        {
            return;
        }

        viewTransform.position = _serializedTree.FindProperty("_graphPosition").vector2Value;
        viewTransform.scale = _serializedTree.FindProperty("_graphScale").vector2Value;
    }

    private void SaveViewTransform(GraphView graphView)
    {
        if (_tree == null)
        {
            return;
        }

        _serializedTree.Update();
        _serializedTree.FindProperty("_graphPosition").vector2Value = viewTransform.position;
        _serializedTree.FindProperty("_graphScale").vector2Value = viewTransform.scale;
        _serializedTree.ApplyModifiedPropertiesWithoutUndo();
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.Where(endPort =>
        endPort.direction != startPort.direction &&
        endPort.node != startPort.node).ToList();
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
                _blackboardView.DeleteProperty(blackboardField);
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

    public MoshitinEncoded.AI.Node CreateNode(System.Type nodeType, string nodeName, Vector2 position, bool includeUndo = true)
    {
        // Creates the node
        var node = ScriptableObject.CreateInstance(nodeType) as MoshitinEncoded.AI.Node;

        if (node == null)
        {
            return null;
        }

        node.name = nodeName;
        node.guid = GUID.Generate().ToString();
        node.position = position;
        //node.hideFlags = HideFlags.HideInHierarchy;

        // Adds the node to the project
        if (!Application.isPlaying)
        {
            AssetDatabase.AddObjectToAsset(node, _tree);
        }

        if (includeUndo)
        {
            Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");
        }

        // Adds the node to the list
        _serializedTree.Update();
        
        var nodesProperty = _serializedTree.FindProperty("m_Nodes");
        nodesProperty.AddToObjectArray(node);

        if (includeUndo)
        {
            _serializedTree.ApplyModifiedProperties();
        }
        else
        {
            _serializedTree.ApplyModifiedPropertiesWithoutUndo();
        }
        
        CreateNodeView(node);

        return node;
    }

    private void DeleteNode(MoshitinEncoded.AI.Node node)
    {
        if (node is RootNode)
        {
            return;
        }

        _serializedTree.Update();

        var nodesProperty = _serializedTree.FindProperty("m_Nodes");
        var nodeRemoved = nodesProperty.RemoveFromObjectArray(node);

        if (nodeRemoved)
        {
            _serializedTree.ApplyModifiedProperties();
            Undo.DestroyObjectImmediate(node);
        }
    }

    private void CreateNodeView(MoshitinEncoded.AI.Node node)
    {
        var nodeView = new NodeView(node);
        AddElement(nodeView);
    }

    public void UpdateNodeStates()
    {
        nodes.ForEach(node =>
        {
            var nodeView = node as NodeView;
            nodeView.UpdateState();
        });
    }
}