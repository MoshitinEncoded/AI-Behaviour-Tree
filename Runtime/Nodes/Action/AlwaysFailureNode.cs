using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Task/Condition/Always Failure")]
    [Tooltip("Returns failure, always.")]
    public class AlwaysFailureNode : TaskNode
    {
        protected override NodeState Run(BehaviourTreeRunner runner)
        {
            return NodeState.Failure;
        }
    }
}
