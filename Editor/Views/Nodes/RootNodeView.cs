using UnityEditor.Experimental.GraphView;

using Node = MoshitinEncoded.AI.BehaviourTreeLib.Node;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class RootNodeView : DecoratorNodeView
    {
        protected override bool ShowInputPort => false;
        
        public RootNodeView(Node node, BehaviourTreeView treeView) : base(node, treeView)
        {
            DisableDelete();
            DisableCopy();
            DisableRename();
            title = "Root";
        }

        private void DisableDelete() => capabilities &= ~Capabilities.Deletable;

        private void DisableCopy() => capabilities &= ~Capabilities.Copiable;

        private void DisableRename() => capabilities &= ~Capabilities.Renamable;

        protected override void AddStyleClass()
        {
            AddToClassList("root");
        }
    }
}
