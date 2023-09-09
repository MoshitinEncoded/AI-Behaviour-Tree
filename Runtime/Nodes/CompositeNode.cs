using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoshitinEncoded.AI
{
    public abstract class CompositeNode : Node
    {
        [HideInInspector] public List<Node> children = new();

        public override Node Clone()
        {
            CompositeNode node = Instantiate(this);
            node.children = children.ConvertAll(child => child.Clone());

            return node;
        }
    }
}