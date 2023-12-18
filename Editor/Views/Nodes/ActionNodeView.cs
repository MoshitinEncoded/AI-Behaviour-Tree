using MoshitinEncoded.AI.BehaviourTreeLib;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class ActionNodeView : NodeView
    {
        protected override bool ShowOutputPort => false;
        
        public ActionNodeView(Node node, BehaviourTreeView treeView) : base(node, treeView) {}

        protected override void AddStyleClass()
        {
            AddToClassList("action");
        }
    }
}
