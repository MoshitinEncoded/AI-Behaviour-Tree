using MoshitinEncoded.AI.BehaviourTreeLib;
using Node = MoshitinEncoded.AI.BehaviourTreeLib.Node;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal abstract class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        protected bool _IsStateVisible = false;
        private static readonly float _HideDelaySeconds = 0.2f;
        private NodeState _PrevState = NodeState.Running;

        public abstract Node Node { get; }

        public Port Input { get; protected set; }

        public Port Output { get; protected set; }

        public NodeView(string uiFile) : base(uiFile)
        {
            AddStyleClass();
        }

        public virtual void AddChild(Node child) { }

        public virtual void RemoveChild(Node child) { }

        public virtual void OnMoved() { }

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

        internal virtual void RemoveNullChilds() { }

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

    internal abstract class NodeView<T> : NodeView where T : Node
    {
        [SerializeField] private T _Node;
        private readonly SerializedObject _SerializedNode;

        public override Node Node => _Node;

        protected T TNode => _Node;

        protected SerializedObject SerializedNode => _SerializedNode;

        protected BehaviourTreeView TreeView { get; private set; }

        public NodeView(T node, BehaviourTreeView treeView) :
            base("Packages/com.moshitin-encoded.ai.behaviourtree/Editor/Views/NodeView.uxml")
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

            CreateInputPort();
            CreateOutputPort();
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            _SerializedNode.Update();

            var positionProperty = _SerializedNode.FindProperty("_Position");
            positionProperty.vector2Value = newPos.position;

            _SerializedNode.ApplyModifiedProperties();
        }

        protected virtual void CreateInputPort()
        {
            Input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));

            Input.portName = "";
            inputContainer.Add(Input);
        }

        protected virtual void CreateOutputPort()
        {
            Output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));

            Output.portName = "";
            outputContainer.Add(Output);
        }

        private void BindTitleLabel()
        {
            var titleLabel = titleContainer.Q<Label>();
            titleLabel.bindingPath = "_Title";
            titleLabel.Bind(_SerializedNode);
        }
    }
}