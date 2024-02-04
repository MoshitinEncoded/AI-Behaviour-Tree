using Node = MoshitinEncoded.AI.BehaviourTreeLib.Node;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class MissingNodeView : CompositeNodeView
    {
        public MissingNodeView(Node node, BehaviourTreeView treeView) : base(node, treeView)
        {
        }

        protected override void AddStyleClass()
        {
            AddToClassList("missing");
        }
    }
}
