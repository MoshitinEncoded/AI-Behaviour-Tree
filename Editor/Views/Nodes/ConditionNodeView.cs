using MoshitinEncoded.AI.BehaviourTreeLib;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class ConditionNodeView : NodeView
    {
        public ConditionNodeView(Node node, BehaviourTreeView treeView) : base(node, treeView) {}

        protected override void AddStyleClass()
        {
            AddToClassList("condition");
        }

        protected override void CreateOutputPort() { }
    }
}
