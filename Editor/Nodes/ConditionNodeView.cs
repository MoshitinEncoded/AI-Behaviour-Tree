using MoshitinEncoded.BehaviourTree;

namespace MoshitinEncoded.Editor.BehaviourTree
{
    internal class ConditionNodeView : NodeView
    {
        public ConditionNodeView(Node node, BehaviourTreeView treeView) : base(node, treeView) {}

        protected override void AddStyleClass()
        {
            AddToClassList("condition");
        }
    }
}
