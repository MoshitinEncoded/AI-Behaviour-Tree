using System.Collections.Generic;
using UnityEngine;

namespace MoshitinEncoded.BehaviourTree
{
    public abstract class DecoratorNode : Node, IParentNode
    {
        [SerializeField, HideInInspector] private Node _Child;

        public Node Child
        {
            get => _Child;
            protected set => _Child = value;
        }

        public void AddChild(Node child)
        {
            if (child == null)
            {
                return;
            }

            _Child = child;
        }

        public void ClearChildren()
        {
            _Child = null;
        }

        public override Node Clone(bool withChild)
        {
            DecoratorNode node = Instantiate(this);
            if (withChild && Child != null)
            {
                node._Child = Child.Clone();
            }
            else
            {
                node._Child = null;
            }

            return node;
        }

        public List<Node> GetChildren()
        {
            if (Child != null)
            {
                return new List<Node>() { Child };
            }

            return new List<Node>();
        }
    }
}