using System.Collections.Generic;
using UnityEngine;

namespace MoshitinEncoded.BehaviourTree
{
    public abstract class CompositeNode : Node, IParentNode
    {
        [HideInInspector] public List<Node> children = new();

        public void AddChild(Node child)
        {
            if (child == null)
            {
                return;
            }

            children.Add(child);
        }

        public override Node Clone(bool withChildren)
        {
            CompositeNode node = Instantiate(this);
            if (withChildren)
            {
                node.children = children.ConvertAll(child => child.Clone());
            }
            else
            {
                node.children.Clear();
            }

            return node;
        }

        public List<Node> GetChildren() => children;
    }
}