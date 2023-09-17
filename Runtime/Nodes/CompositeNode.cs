using System.Collections.Generic;
using UnityEngine;

namespace MoshitinEncoded.AI
{
    public abstract class CompositeNode : Node
    {
        [HideInInspector] public List<Node> children = new();

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
    }
}