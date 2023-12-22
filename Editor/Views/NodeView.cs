using System;
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
        private readonly Label _TitleLabel;
        private readonly TextField _TitleTextField;

        public Node Node => _Node;

        public SerializedObject SerializedNode => _SerializedNode;

        public Port Input { get; protected set; }

        public Port Output { get; protected set; }

        protected SerializedProperty ChildrenProperty => _ChildrenProperty;

        protected BehaviourTreeView TreeView { get; private set; }

        protected virtual bool ShowInputPort { get; } = true;

        protected virtual bool ShowOutputPort { get; } = true;

        public NodeView(Node node, BehaviourTreeView treeView) : base("Packages/com.moshitin-encoded.ai.behaviourtree/Editor/Views/NodeView.uxml")
        {
            _Node = node;
            _SerializedNode = new SerializedObject(node);
            _ChildrenProperty = _SerializedNode.FindProperty("_Children");
            TreeView = treeView;

            _TitleLabel = this.Q<Label>("title-label");
            _TitleTextField = this.Q<TextField>("title-text-field");

            AddStyleClass();

            viewDataKey = _SerializedNode.FindProperty("_Guid").stringValue;

            base.SetPosition(new Rect
            {
                position = _SerializedNode.FindProperty("_Position").vector2Value
            });

            AddInputPort();
            AddOutputPort();

            capabilities |= Capabilities.Renamable;

            BindTitleLabel();
            SetupTitleTextField();
        }

        private void OnTitleEditFinished()
        {
            if (title != _TitleTextField.text)
            {
                title = _TitleTextField.text;

                _SerializedNode.Update();
                _SerializedNode.FindProperty("_Title").stringValue = title;
                _SerializedNode.ApplyModifiedProperties();
            }

            _TitleLabel.style.display = DisplayStyle.Flex;
            _TitleTextField.style.display = DisplayStyle.None;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.AppendAction("Rename", (evt) => OpenTextEditor(), IsRenamable() ? DropdownMenuAction.AlwaysEnabled : DropdownMenuAction.AlwaysDisabled);
        }

        private void OpenTextEditor()
        {
            SerializedNode.Update();
            var title = SerializedNode.FindProperty("_Title").stringValue;

            _TitleTextField.SetValueWithoutNotify(title);
            _TitleTextField.style.display = DisplayStyle.Flex;
            _TitleLabel.style.display = DisplayStyle.None;
            _TitleTextField.Q(TextInputBaseField<string>.textInputUssName).Focus();
            _TitleTextField.SelectAll();
        }

        public virtual void AddChild(Node child) { }

        public virtual void RemoveChild(Node child) { }

        public virtual void OnGraphElementMoved() { }

        public override void OnSelected()
        {
            base.OnSelected();
            Selection.activeObject = Node;
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

        private void AddInputPort()
        {
            if (!ShowInputPort)
            {
                return;
            }

            Input = CreateInputPort();

            Input.portName = "";
            inputContainer.Add(Input);
        }

        private Port CreateInputPort()
        {
            var portType = GetInputPortType();
            return InstantiatePort(portType.Orientation, Direction.Input, portType.Capacity, typeof(bool));
        }

        protected virtual PortType GetInputPortType() => new(Orientation.Vertical, Port.Capacity.Single);

        private void AddOutputPort()
        {
            if (!ShowOutputPort)
            {
                return;
            }

            Output = CreateOutputPort();

            Output.portName = "";
            outputContainer.Add(Output);
        }

        private Port CreateOutputPort()
        {
            var portType = GetOutputPortType();
            return InstantiatePort(portType.Orientation, Direction.Output, portType.Capacity, typeof(bool));
        }

        protected virtual PortType GetOutputPortType() => new(Orientation.Vertical, Port.Capacity.Single);

        private void BindTitleLabel()
        {
            _TitleLabel.bindingPath = "_Title";
            _TitleLabel.Bind(_SerializedNode);
        }

        private void SetupTitleTextField()
        {
            _TitleTextField.Q<Label>().style.display = DisplayStyle.None;
            _TitleTextField.style.display = DisplayStyle.None;

            var inputTextField = _TitleTextField.Q(TextInputBaseField<string>.textInputUssName);
            inputTextField.RegisterCallback<FocusOutEvent>(delegate { OnTitleEditFinished(); }, TrickleDown.TrickleDown);
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

        protected struct PortType
        {
            public PortType(Orientation orientation, Port.Capacity capacity)
            {
                Orientation = orientation;
                Capacity = capacity;
            }

            public Orientation Orientation;
            public Port.Capacity Capacity;
        }
    }
}