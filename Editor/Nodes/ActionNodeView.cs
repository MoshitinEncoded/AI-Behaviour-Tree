using MoshitinEncoded.BehaviourTree;

namespace MoshitinEncoded.Editor.BehaviourTree
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
