using System.Collections.Generic;
using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    public abstract class CompositeNode : Node, IParentNode
    {
        [SerializeField, HideInInspector] private List<Node> _Children = new();

        public List<Node> Children
        {
            get => _Children;
            private set => _Children = value;
        }

        public void AddChild(Node child)
        {
            if (child == null)
            {
                return;
            }

            Children.Add(child);
        }

        public void ClearChildren()
        {
            Children.Clear();
        }

        public override Node Clone(bool withChildren)
        {
            CompositeNode node = Instantiate(this);
            if (withChildren)
            {
                node.Children = Children.ConvertAll(child => child.Clone());
            }
            else
            {
                node.Children.Clear();
            }

            return node;
        }

        public List<Node> GetChildren() => Children;
    }
}