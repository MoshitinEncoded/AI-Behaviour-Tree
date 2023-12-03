using MoshitinEncoded.AI.BehaviourTreeLib;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class ActionNodeView : NodeView<ActionNode>
    {
        public ActionNodeView(ActionNode node, BehaviourTreeView treeView) : base(node, treeView) {}

        protected override void AddStyleClass()
        {
            AddToClassList("action");
        }

        protected override void CreateOutputPort() { }
    }
}
