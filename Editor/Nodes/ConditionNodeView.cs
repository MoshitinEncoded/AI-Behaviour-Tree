using MoshitinEncoded.AIBehaviourTree;

namespace MoshitinEncoded.Editor.AIBehaviourTree
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
