using System.Collections.Generic;
using UnityEngine;

namespace MoshitinEncoded.BehaviourTree
{
    public class RootNode : Node, IParentNode
    {
        [HideInInspector] public Node child;
        protected override void OnStart()
        {
            
        }

        protected override void OnStop()
        {
            
        }

        protected override State OnUpdate() =>
            child.Update();

        public override Node Clone(bool withChild)
        {
            RootNode rootNode = Instantiate(this);
            if (child != null)
            {
                rootNode.child = child.Clone();
            }
            
            return rootNode;
        }

        public void AddChild(Node child)
        {
            if (child != null)
            {
                this.child = child;
            }
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