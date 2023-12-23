using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [Tooltip("The root of all evil... of your behaviour tree.")]
    public class RootNode : NodeBehaviour
    {
        public Node Child => Node.Children.Length > 0 ? Node.Children[0] : null;

        protected override NodeState Run(BehaviourTreeRunner runner) =>
            (!Child) ? NodeState.Failure : Child.RunBehaviour(runner);
    }
}