using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Task/Condition/Always Success")]
    [Tooltip("Returns success, always.")]
    public class AlwaysSuccessNode : TaskNode
    {
        protected override NodeState Run(BehaviourTreeRunner runner)
        {
            return NodeState.Success;
        }
    }
}
