using MoshitinEncoded.AI.BehaviourTreeLib;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class ConditionNodeView : NodeView
    {
        protected override bool ShowOutputPort => false;
        
        public ConditionNodeView(Node node, BehaviourTreeView treeView) : base(node, treeView) {}

        protected override void AddStyleClass()
        {
            AddToClassList("condition");
        }
    }
}
