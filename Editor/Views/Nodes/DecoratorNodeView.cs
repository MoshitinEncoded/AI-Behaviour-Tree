using MoshitinEncoded.AI.BehaviourTreeLib;
using UnityEditor;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class DecoratorNodeView : NodeView<DecoratorNode>
    {
        private readonly SerializedProperty _ChildProperty;
        
        public DecoratorNodeView(DecoratorNode node, BehaviourTreeView treeView) : base(node, treeView)
        {
            _ChildProperty = SerializedNode.FindProperty("_Child");
        }

        public override void AddChild(Node child) => SetChild(child);

        public override void RemoveChild(Node child) => SetChild(null);

        private void SetChild(Node child)
        {
            SerializedNode.Update();
            _ChildProperty.objectReferenceValue = child;
            SerializedNode.ApplyModifiedProperties();
        }

        protected override void AddStyleClass()
        {
            AddToClassList("decorator");
        }
    }
}
