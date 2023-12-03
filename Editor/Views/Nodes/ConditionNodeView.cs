using MoshitinEncoded.AI.BehaviourTreeLib;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class ConditionNodeView : NodeView<ConditionNode>
    {
        public ConditionNodeView(ConditionNode node, BehaviourTreeView treeView) : base(node, treeView) {}

        protected override void AddStyleClass()
        {
            AddToClassList("condition");
        }
    }
}
