using MoshitinEncoded.AI.BehaviourTreeLib;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class DecoratorNodeView : NodeView
    {   
        public DecoratorNodeView(Node node, BehaviourTreeView treeView) : base(node, treeView) { }

        protected override void AddStyleClass()
        {
            AddToClassList("decorator");
        }

        public override void AddChild(Node child)
        {
            if (HasChild())
            {
                return;
            }

            SerializedNode.Update();
            ChildrenProperty.AddToArray(child);
            SerializedNode.ApplyModifiedProperties();
        }

        public override void RemoveChild(Node child)
        {
            if (!IsParentOf(child))
            {
                return;
            }

            SerializedNode.Update();
            ChildrenProperty.DeleteArrayElementAtIndex(0);
            SerializedNode.ApplyModifiedProperties();
        }

        private bool IsParentOf(Node child)
        {
            return HasChild() && ChildrenProperty.GetArrayElementAtIndex(0).objectReferenceValue == child;
        }

        private bool HasChild()
        {
            return ChildrenProperty.arraySize > 0;
        }
    }
}
