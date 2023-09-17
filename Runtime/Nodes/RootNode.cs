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

        public override Node Clone(bool withChild)
        {
            RootNode rootNode = Instantiate(this);
            if (child != null)
            {
                rootNode.child = child.Clone();
            }
            
            return rootNode;
        }
    }
}