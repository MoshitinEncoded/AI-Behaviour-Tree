using MoshitinEncoded.AIBehaviourTree;
using UnityEditor;

namespace MoshitinEncoded.Editor.AIBehaviourTree
{
    internal class DecoratorNodeView : NodeView
    {
        private readonly SerializedProperty _ChildProperty;
        
        public DecoratorNodeView(Node node, BehaviourTreeView treeView) : base(node, treeView)
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
