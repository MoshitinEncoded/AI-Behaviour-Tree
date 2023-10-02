using MoshitinEncoded.BehaviourTree;
using Node = MoshitinEncoded.BehaviourTree.Node;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MoshitinEncoded.Editor.BehaviourTree
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        [SerializeField] private Node _node;
        public Node Node => _node;
        public Port Input { get; private set; }
        public Port Output { get; private set; }

        private readonly SerializedObject _serializedNode;

        public NodeView(Node node) : base("Packages/com.moshitin-encoded.behaviourgraph/Editor/NodeView.uxml")
        {
            _node = node;

            title = node.name;
            viewDataKey = node.guid;
            if (node is RootNode)
            {
                // Removes the capability of being deleted
                capabilities &= ~Capabilities.Deletable;
            }
            
            _serializedNode = new SerializedObject(node);

            if (node is not RootNode)
            {
                var titleLabel = titleContainer.Q<Label>();
                titleLabel.bindingPath = "Title";
                titleLabel.Bind(_serializedNode);
            }
            
            var newPosition = new Rect
            {
                position = node.position
            };
            base.SetPosition(newPosition);
            //style.left = node.position.x;
            //style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            AddStyleClass();
        }

        public void AddChild(Node child)
        {
            _serializedNode.Update();

            switch (Node)
            {
                case RootNode:
                case DecoratorNode:
                {
                    var childProperty = _serializedNode.FindProperty("child");
                    childProperty.objectReferenceValue = child;
                    break;
                }
                case CompositeNode:
                {
                    var childrenProperty = _serializedNode.FindProperty("children");
                    childrenProperty.AddToObjectArray(child);
                    break;
                }
            }

            _serializedNode.ApplyModifiedProperties();

            SortChildren();
        }

        public void RemoveChild(Node child)
        {
            _serializedNode.Update();

            switch (Node)
            {
                case RootNode:
                case DecoratorNode:
                {
                    var childProperty = _serializedNode.FindProperty("child");
                    childProperty.objectReferenceValue = null;
                    break;
                }
                case CompositeNode:
                {
                    var childrenProperty = _serializedNode.FindProperty("children");
                    childrenProperty.RemoveFromObjectArray(child);
                    break;
                }
            }

            _serializedNode.ApplyModifiedProperties();
        }

        private void CreateInputPorts()
        {
            switch (Node)
            {
                case ActionNode:
                case CompositeNode:
                case DecoratorNode:
                    Input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    break;
            }

            if (Input == null)
            {
                return;
            }

            Input.portName = "";
            inputContainer.Add(Input);
        }

        private void CreateOutputPorts()
        {
            switch (Node)
            {
                case CompositeNode:
                    Output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    Output.portName = "Children";
                    break;
                case DecoratorNode:
                case RootNode:
                    Output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    Output.portName = "Child";
                    break;
            }

            if (Output != null)
            {
                outputContainer.Add(Output);
            }
        }

        private void AddStyleClass()
        {
            switch (Node)
            {
                case ActionNode:
                    AddToClassList("action");
                    break;
                case CompositeNode:
                    AddToClassList("composite");
                    break;
                case DecoratorNode:
                    AddToClassList("decorator");
                    break;
                case RootNode:
                    AddToClassList("root");
                    break;
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            _serializedNode.Update();

            var posProperty = _serializedNode.FindProperty("position");
            posProperty.vector2Value = newPos.position;

            _serializedNode.ApplyModifiedProperties();
        }

        public override void OnSelected()
        {
            base.OnSelected();
            Selection.activeObject = Node;
        }

        public void SortChildren()
        {
            if (Node is CompositeNode composite)
            {
                composite.children.Sort(SortByHorizontalPosition);
            }
        }

        private static int SortByHorizontalPosition(Node left, Node right) =>
            left.position.x < right.position.x ? -1 : 1;

        public void UpdateState()
        {
            RemoveFromClassList("running");
            RemoveFromClassList("failure");
            RemoveFromClassList("success");

            switch (Node.state)
            {
                case Node.State.Running:
                    if (Node.Started)
                        AddToClassList("running");
                    break;
                case Node.State.Failure:
                    AddToClassList("failure");
                    break;
                case Node.State.Success:
                    AddToClassList("success");
                    break;
            }
        }
    }
}