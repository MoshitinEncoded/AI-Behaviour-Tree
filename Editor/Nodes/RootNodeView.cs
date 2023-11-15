using UnityEditor.Experimental.GraphView;
using Node = MoshitinEncoded.AI.BehaviourTreeLib.Node;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class RootNodeView : NodeView
    {
        public RootNodeView(Node node, BehaviourTreeView treeView) : base(node, treeView)
        {
            DisableDelete();
            title = "Root";
        }

        public override void AddChild(Node child) => SetChild(child);

        public override void RemoveChild(Node child) => SetChild(null);

        private void SetChild(Node child)
        {
            SerializedNode.Update();
            var childProperty = SerializedNode.FindProperty("_Child");
            childProperty.objectReferenceValue = child;
            SerializedNode.ApplyModifiedProperties();
        }

        protected override void AddStyleClass()
        {
            AddToClassList("root");
        }

        private void DisableDelete()
        {
            capabilities &= ~Capabilities.Deletable;
        }
    }
}
