using MoshitinEncoded.AIBehaviourTree;
using Node = MoshitinEncoded.AIBehaviourTree.Node;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace MoshitinEncoded.Editor.AIBehaviourTree
{
    internal class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        protected bool _IsStateVisible = false;
        [SerializeField] private Node _Node;
        private readonly SerializedObject _SerializedNode;
        private static readonly float _HideDelaySeconds = 0.2f;
        private NodeState _PrevState = NodeState.Running;

        public Node Node => _Node;

        public Port Input { get; private set; }

        public Port Output { get; private set; }

        protected BehaviourTreeView TreeView { get; private set; }

        protected SerializedObject SerializedNode => _SerializedNode;

        public NodeView(Node node, BehaviourTreeView treeView) : base("Packages/com.moshitin-encoded.aibehaviourtree/Editor/NodeView.uxml")
        {
            _Node = node;
            _SerializedNode = new SerializedObject(node);
            TreeView = treeView;

            viewDataKey = _SerializedNode.FindProperty("_Guid").stringValue;

            if (node is not RootNode)
            {
                BindTitleLabel();
            }

            base.SetPosition(new Rect
            {
                position = _SerializedNode.FindProperty("_Position").vector2Value
            });

            CreateInputPorts();
            CreateOutputPorts();
            AddStyleClass();
        }

        public virtual void AddChild(Node child) { }

        public virtual void RemoveChild(Node child) { }

        public virtual void OnMoved() { }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            _SerializedNode.Update();

            var positionProperty = _SerializedNode.FindProperty("_Position");
            positionProperty.vector2Value = newPos.position;

            _SerializedNode.ApplyModifiedProperties();
        }

        public override void OnSelected()
        {
            base.OnSelected();
            Selection.activeObject = Node;
        }

        internal void UpdateState()
        {
            if (IsHideDelayCompleted())
            {
                HideStates();
                return;
            }

            if (!WasCompletedThisFrame())
            {
                ShowState(Node.State);
            }

            _PrevState = Node.State;
        }

        protected virtual void AddStyleClass() { }

        protected virtual void OnUpdateState() { }

        protected List<Node> GetChildren()
        {
            if (Node is IParentNode parentNode)
            {
                return parentNode.GetChildren();
            }
            else
            {
                return new List<Node>();
            }
        }

        private void BindTitleLabel()
        {
            var titleLabel = titleContainer.Q<Label>();
            titleLabel.bindingPath = "_Title";
            titleLabel.Bind(_SerializedNode);
        }

        private void CreateInputPorts()
        {
            if (Node is not RootNode)
            {
                Input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
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

        private void ShowState(NodeState nodeState)
        {
            HideStates();
            switch (nodeState)
            {
                case NodeState.Running:
                    AddToClassList("running");
                    break;
                case NodeState.Failure:
                    AddToClassList("failure");
                    break;
                case NodeState.Success:
                    AddToClassList("success");
                    break;
            }
        }

        private void HideStates()
        {
            RemoveFromClassList("running");
            RemoveFromClassList("failure");
            RemoveFromClassList("success");
        }

        private bool WasCompletedThisFrame() => Node.State != _PrevState && _PrevState == NodeState.Running;

        private bool IsHideDelayCompleted() => Time.time - Node.LastUpdateTime >= _HideDelaySeconds;
    }
}