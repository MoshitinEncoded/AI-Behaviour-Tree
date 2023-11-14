using MoshitinEncoded.AIBehaviourTree;

namespace MoshitinEncoded.Editor.AIBehaviourTree
{
    internal class ActionNodeView : NodeView
    {
        public ActionNodeView(Node node, BehaviourTreeView treeView) : base(node, treeView) {}

        protected override void AddStyleClass()
        {
            AddToClassList("action");
        }
    }
}
