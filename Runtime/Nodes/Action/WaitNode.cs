using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Action/Wait")]
    public class WaitNode : ActionNode
    {
        [Space]
        [Tooltip("Time in seconds to wait.")]
        [SerializeField] private float _Time = 1f;

        public float Time => _Time;

        protected override NodeState Run(BehaviourTreeRunner runner)
        {
            if (IsTimeOver())
            {
                return NodeState.Success;
            }

            return NodeState.Running;
        }

        private bool IsTimeOver() =>
            UnityEngine.Time.time - StartTime >= _Time;
    }
}