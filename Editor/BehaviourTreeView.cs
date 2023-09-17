using MoshitinEncoded.AI;
using MoshitinEncoded.AI.Editor;
using MoshitinEncoded.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Serialization.Json;
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

    private Vector2 _mousePosition;

    private void UpdateMousePosition(MouseMoveEvent evt)
    {
        _mousePosition = contentViewContainer.WorldToLocal(evt.mousePosition);
    }

    public BehaviourTreeView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.moshitin-encoded.tree-ai/Editor/BehaviourTreeEditor.uss");
        styleSheets.Add(styleSheet);
        serializeGraphElements += CopyElements;
        unserializeAndPaste += PasteElements;

        Undo.undoRedoPerformed += OnUndoRedo;
        viewTransformChanged += SaveViewTransform;

        RegisterCallback<MouseMoveEvent>(UpdateMousePosition);
    }

    [Serializable]
    private class CopySelectionData
    {
        public Rect SelectionRect;
        public NodeCopyData[] NodeCopiesData;
    }

    [Serializable]
    private class NodeCopyData
    {
        public NodeCopyData() {}

        public NodeCopyData(MoshitinEncoded.AI.Node node, string[] childGuids)
        {
            Node = node;
            ChildGuids = childGuids;
        }

        public MoshitinEncoded.AI.Node Node;
        public string[] ChildGuids;
    }

    private string CopyElements(IEnumerable<GraphElement> elements)
    {
        var nodeViews = new List<NodeView>();
        var edges = new List<Edge>();

        foreach (var element in elements)
        {
            if (element is NodeView nodeView && nodeView.Node is not RootNode)
            {
                nodeViews.Add(nodeView);
            }
            else if (element is Edge edge)
            {
                if (edge.input.node != null && 
                    edge.output.node != null && 
                    elements.Contains(edge.input.node) && 
                    elements.Contains(edge.output.node) && 
                    (edge.output.node as NodeView).Node is not RootNode)
                {
                    edges.Add(edge);
                }
            }
        }

        var copies = new Dictionary<NodeView, MoshitinEncoded.AI.Node>();

        foreach (var originalNodeView in nodeViews)
        {
            var nodeCopy = originalNodeView.Node.Clone(withChildren: false);
            nodeCopy.guid = GUID.Generate().ToString();
            nodeCopy.name = nodeCopy.name.Remove(nodeCopy.name.IndexOf('('));

            copies.Add(
                key: originalNodeView,
                value: nodeCopy
            );
        }

        var copiesData = new List<NodeCopyData>();

        foreach (var copy in copies)
        {
            var childGuids = new List<string>();
            var connectedEdges = edges.Where(edge => edge.output.node == copy.Key);
            foreach (var connectedEdge in connectedEdges)
            {
                var childNodeView = (NodeView)connectedEdge.input.node;
                var childCopy = copies[childNodeView];
                childGuids.Add(childCopy.guid);
            }

            copiesData.Add(new NodeCopyData(copy.Value, childGuids.ToArray()));
        }

        var copySelectionData = new CopySelectionData()
        {
            NodeCopiesData = copiesData.ToArray(),
            SelectionRect = CalculateRectToFitElements(nodeViews.ToArray())
        };

        var parameters = new JsonSerializationParameters()
        {
            UserDefinedAdapters = new List<IJsonAdapter>() { new CopyNodeJsonAdapter() }
        };

        return JsonSerialization.ToJson(copySelectionData, parameters);
    }

    private class CopyNodeJsonAdapter : IJsonAdapter<NodeCopyData>
    {
        public NodeCopyData Deserialize(in JsonDeserializationContext<NodeCopyData> context)
        {
            var serializedNode = context.SerializedValue.GetValue("Node");
            var nodeParams = new JsonSerializationParameters()
            {
                DisableRootAdapters = true
            };

            var node = JsonSerialization.FromJson<MoshitinEncoded.AI.Node>(serializedNode, nodeParams);
            var childGuids = JsonSerialization.FromJson<string[]>(context.SerializedValue.GetValue("ChildGuids"));

            return new NodeCopyData(node, childGuids);
        }

        public void Serialize(in JsonSerializationContext<NodeCopyData> context, NodeCopyData value)
        {
            context.Writer.WriteBeginObject();

            var nodeParams = new JsonSerializationParameters()
            {
                DisableRootAdapters = true
            };

            context.Writer.WriteKeyValueLiteral("Node", JsonSerialization.ToJson(value.Node, nodeParams));

            context.Writer.WriteBeginArray("ChildGuids");
            foreach (var nodeGuid in value.ChildGuids)
            {
                context.Writer.WriteValue(nodeGuid);
            }
            context.Writer.WriteEndArray();

            context.Writer.WriteEndObject();
        }
    }

    private Rect CalculateRectToFitElements(GraphElement[] elements)
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

    private void PasteElements(string operationName, string data)
    {
        var parameters = new JsonSerializationParameters()
        {
            UserDefinedAdapters = new List<IJsonAdapter>() { new CopyNodeJsonAdapter() }
        };

        var copySelectionData = JsonSerialization.FromJson<CopySelectionData>(data, parameters);
        var nodeCopies = copySelectionData.NodeCopiesData;

        // Paste nodes
        foreach (var nodeCopy in nodeCopies)
        {
            var node = nodeCopy.Node;
            if (operationName == "Duplicate")
            {
                node.position += new Vector2(10, 10);
            }
            else
            {
                node.position = _mousePosition + node.position - copySelectionData.SelectionRect.center;
            }

            node.name = node.GetType().Name; 
            AddNode(node, registerUndo: true);
        }

        ClearSelection();

        // Add node childs
        foreach (var nodeCopy in nodeCopies)
        {
            var parentView = GetNodeView(nodeCopy.Node);
            AddToSelection(parentView);

            foreach (var childGuid in nodeCopy.ChildGuids)
            {
                var childClone = nodeCopies.First(n => n.Node.guid == childGuid).Node;
                parentView.AddChild(childClone);

                var childCloneView = GetNodeView(childClone);
                var parentChildEdge = parentView.Output.ConnectTo(childCloneView.Input);
                AddElement(parentChildEdge);
            }
        }
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

    public NodeView GetNodeView(MoshitinEncoded.AI.Node node) =>
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
                NodeView parentView = GetNodeView(parentNode);
                NodeView childView = GetNodeView(childNode);

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

    public MoshitinEncoded.AI.Node CreateNode(Type nodeType, string nodeTitle, Vector2 position, bool registerUndo = true)
    {
        // Creates the node
        var node = ScriptableObject.CreateInstance(nodeType) as MoshitinEncoded.AI.Node;

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

    private void AddNode(MoshitinEncoded.AI.Node node, bool registerUndo)
    {
        // Adds the node to the project
        if (!Application.isPlaying)
        {
            AssetDatabase.AddObjectToAsset(node, _tree);
        }

        if (registerUndo)
        {
            Undo.RegisterCreatedObjectUndo(node, "Create Node (Behaviour Tree)");
        }

        // Adds the node to the list
        _serializedTree.Update();
        
        var nodesProperty = _serializedTree.FindProperty("m_Nodes");
        nodesProperty.AddToObjectArray(node);

        if (registerUndo)
        {
            _serializedTree.ApplyModifiedProperties();
        }
        else
        {
            _serializedTree.ApplyModifiedPropertiesWithoutUndo();
        }
        
        CreateNodeView(node);
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