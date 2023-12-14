using MoshitinEncoded.AI.BehaviourTreeLib;

using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

using Node = MoshitinEncoded.AI.BehaviourTreeLib.Node;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal abstract class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        private static readonly float _HideDelaySeconds = 0.2f;

        private readonly Node _Node;
        private readonly SerializedObject _SerializedNode;
        private readonly SerializedProperty _ChildrenProperty;
        private NodeState _PrevState = NodeState.Running;

        public Node Node => _Node;

        public SerializedObject SerializedNode => _SerializedNode;

        public Port Input { get; protected set; }

        public Port Output { get; protected set; }

        protected SerializedProperty ChildrenProperty => _ChildrenProperty;

        protected BehaviourTreeView TreeView { get; private set; }

        public NodeView(Node node, BehaviourTreeView treeView) : base("Packages/com.moshitin-encoded.ai.behaviourtree/Editor/Views/NodeView.uxml")
        {
            _Node = node;
            _SerializedNode = new SerializedObject(node);
            _ChildrenProperty = _SerializedNode.FindProperty("_Children");
            TreeView = treeView;

            AddStyleClass();

            viewDataKey = _SerializedNode.FindProperty("_Guid").stringValue;

            BindTitleLabel();

            base.SetPosition(new Rect
            {
                position = _SerializedNode.FindProperty("_Position").vector2Value
            });

            CreateInputPort();
            CreateOutputPort();
        }

        public virtual void AddChild(Node child) { }

        public virtual void RemoveChild(Node child) { }

        public virtual void OnGraphElementMoved() { }

        public override void OnSelected()
        {
            base.OnSelected();
            Selection.activeObject = Node.Behaviour;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            _SerializedNode.Update();

            var positionProperty = _SerializedNode.FindProperty("_Position");
            positionProperty.vector2Value = newPos.position;

            _SerializedNode.ApplyModifiedProperties();
        }

        internal void UpdateState()
        {
            Debug.Log(Node.name);
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

        private bool IsHideDelayCompleted() => Time.time - Node.LastRunTime >= _HideDelaySeconds;
    }
}