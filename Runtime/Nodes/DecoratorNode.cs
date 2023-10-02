using System.Collections.Generic;
using UnityEngine;

namespace MoshitinEncoded.BehaviourTree
{
    public abstract class DecoratorNode : Node, IParentNode
    {
        [HideInInspector] public Node child;

        public void AddChild(Node child)
        {
            if (child == null)
            {
                return;
            }

            this.child = child;
        }

        public override Node Clone(bool withChild)
        {
            DecoratorNode node = Instantiate(this);
            if (withChild && child != null)
            {
                node.child = child.Clone();
            }
            else
            {
                node.child = null;
            }

            return node;
        }

        public List<Node> GetChildren()
        {
            if (child != null)
            {
                return new List<Node>() { child };
            }

            return new List<Node>();
        }
    }
}