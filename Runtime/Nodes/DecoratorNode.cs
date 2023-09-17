using UnityEngine;

namespace MoshitinEncoded.AI
{
    public abstract class DecoratorNode : Node
    {
        [HideInInspector] public Node child;

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
    }
}