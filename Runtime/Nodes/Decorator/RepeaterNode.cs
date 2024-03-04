using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Decorator/Repeater")]
    [Tooltip("Always returns \"running\".")]
    public class RepeaterNode : DecoratorNode
    {
        protected override NodeState Run(BehaviourTreeRunner runner)
        {
            if (Child) Child.RunBehaviour(runner);
            return NodeState.Running;
        }
    }
}