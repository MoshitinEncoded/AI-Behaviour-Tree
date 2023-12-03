using MoshitinEncoded.AI.BehaviourTreeLib;
using UnityEditor;
using GraphView = UnityEditor.Experimental.GraphView;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class CompositeNodeView : NodeView<CompositeNode>
    {
        private readonly SerializedProperty _ChildrenProperty;

        public CompositeNodeView(CompositeNode node, BehaviourTreeView treeView) : base(node, treeView)
        {
            _ChildrenProperty = SerializedNode.FindProperty("_Children");
        }

        public override void AddChild(Node child)
        {
            SerializedNode.Update();
            _ChildrenProperty.AddToObjectArray(child);
            SerializedNode.ApplyModifiedProperties();
            SortChildren();
        }

        public override void RemoveChild(Node child)
        {
            SerializedNode.Update();
            _ChildrenProperty.RemoveFromObjectArray(child);
            SerializedNode.ApplyModifiedProperties();
        }

        public override void OnMoved() => SortChildren();

        internal override void RemoveNullChilds()
        {
            SerializedNode.Update();
            var children = TNode.Children;
            
            for (var i = children.Count - 1; i >= 0; i--)
            {
                if (children[i] == null)
                {
                    _ChildrenProperty.DeleteArrayElementAtIndex(i);
                }
            }
            
            SerializedNode.ApplyModifiedPropertiesWithoutUndo();
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

        protected override void AddStyleClass()
        {
            AddToClassList("composite");
        }

        private void SortChildren()
        {
            TNode.Children.Sort(SortByHorizontalPosition);
        }

        private static int SortByHorizontalPosition(Node left, Node right)
        {
            var leftPos = new SerializedObject(left).FindProperty("_Position").vector2Value;
            var rightPos = new SerializedObject(right).FindProperty("_Position").vector2Value;
            return leftPos.x < rightPos.x ? -1 : 1;
        }
    }
}
