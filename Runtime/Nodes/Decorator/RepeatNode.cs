using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Decorator/Repeater")]
    [Tooltip("Runs its child forever.")]
    public class RepeatNode : DecoratorNode
    {
        protected override NodeState Run(BehaviourTreeRunner runner)
        {
            if (Child) Child.RunBehaviour(runner);
            return NodeState.Running;
        }
    }
}