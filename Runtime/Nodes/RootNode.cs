using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoshitinEncoded.AI
{
    public class RootNode : Node
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

        public override Node Clone()
        {
            RootNode rootNode = Instantiate(this);
            rootNode.child = child.Clone();
            return rootNode;
        }
    }
}