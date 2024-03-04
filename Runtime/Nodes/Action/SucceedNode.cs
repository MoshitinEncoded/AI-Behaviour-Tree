using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Task/Debug/Succeed")]
    [Tooltip("Succeeds or not. Useful to enable/disable behaviours at runtime.")]
    public class SucceedNode : TaskNode
    {
        [Space]
        [SerializeField] private bool _Succeed = true;

        protected override NodeState Run(BehaviourTreeRunner runner)
        {
            if (_Succeed)
            {
                return NodeState.Success;
            }
            else
            {
                return NodeState.Failure;
            }
        }
    }
}
