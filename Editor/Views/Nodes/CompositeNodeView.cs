using System.Linq;

using MoshitinEncoded.AI.BehaviourTreeLib;

using UnityEditor;

using GraphView = UnityEditor.Experimental.GraphView;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class CompositeNodeView : NodeView
    {
        public CompositeNodeView(Node node, BehaviourTreeView treeView) : base(node, treeView) { }

        protected override void AddStyleClass()
        {
            AddToClassList("composite");
        }

        protected override void CreateOutputPort()
        {
            Output = InstantiatePort(
                GraphView.Orientation.Vertical,
                GraphView.Direction.Output,
                GraphView.Port.Capacity.Multi,
                typeof(bool));

            Output.portName = "";
            outputContainer.Add(Output);
        }

        public override void AddChild(Node child)
        {
            SerializedNode.Update();
            ChildrenProperty.AddToArray(child);
            SerializedNode.ApplyModifiedProperties();
            SortChildren();
        }

        public override void RemoveChild(Node child)
        {
            SerializedNode.Update();
            var nodeRemoved = ChildrenProperty.RemoveFromArray(child);
            
            if (nodeRemoved)
            {
                SerializedNode.ApplyModifiedProperties();
            }
        }

        public override void OnGraphElementMoved() => SortChildren();

        private void SortChildren()
        {
            var orderedChildren = Node.Children.OrderBy(GetNodeXPosition);
            SetChildren(orderedChildren.ToArray());
        }

        private void SetChildren(Node[] newChildren)
        {
            SerializedNode.Update();
            ChildrenProperty.ReplaceArray(newChildren);
            SerializedNode.ApplyModifiedProperties();
        }

        private static float GetNodeXPosition(Node node)
        {
            var pos = new SerializedObject(node).FindProperty("_Position").vector2Value;
            return pos.x;
        }
    }
}
