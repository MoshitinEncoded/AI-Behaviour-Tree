using UnityEditor.Experimental.GraphView;

using Node = MoshitinEncoded.AI.BehaviourTreeLib.Node;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class RootNodeView : DecoratorNodeView
    {
        public RootNodeView(Node node, BehaviourTreeView treeView) : base(node, treeView)
        {
            DisableDelete();
            title = "Root";
        }

        protected override void AddStyleClass()
        {
            AddToClassList("root");
        }

        protected override void CreateInputPort() { }

        private void DisableDelete()
        {
            capabilities &= ~Capabilities.Deletable;
        }
    }
}
