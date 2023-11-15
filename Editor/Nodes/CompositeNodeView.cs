using MoshitinEncoded.AI.BehaviourTreeLib;
using UnityEditor;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class CompositeNodeView : NodeView
    {
        private readonly SerializedProperty _ChildrenProperty;

        public CompositeNodeView(Node node, BehaviourTreeView treeView) : base(node, treeView)
        {
            _ChildrenProperty = SerializedNode.FindProperty("_Children");
        }

        public override void AddChild(Node child)
        {
            SerializedNode.Update();
            _ChildrenProperty.AddToObjectArray(child);
            SerializedNode.ApplyModifiedProperties();
            SortChildren();
        }

        public override void RemoveChild(Node child)
        {
            SerializedNode.Update();
            _ChildrenProperty.RemoveFromObjectArray(child);
            SerializedNode.ApplyModifiedProperties();
        }

        public override void OnMoved() => SortChildren();

        protected override void AddStyleClass()
        {
            AddToClassList("composite");
        }

        private void SortChildren()
        {
            var compositeNode = Node as CompositeNode;
            compositeNode.Children.Sort(SortByHorizontalPosition);
        }

        private static int SortByHorizontalPosition(Node left, Node right)
        {
            var leftPos = new SerializedObject(left).FindProperty("_Position").vector2Value;
            var rightPos = new SerializedObject(right).FindProperty("_Position").vector2Value;
            return leftPos.x < rightPos.x ? -1 : 1;
        }
    }
}
