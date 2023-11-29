using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Action/Wait")]
    public class WaitNode : ActionNode
    {
        [Space]
        [Tooltip("Time in seconds to wait.")]
        [SerializeField] private float _Time = 1f;

        private float _StartTime;

        public float Time => _Time;

        protected override void OnStart(BehaviourTreeRunner runner)
        {
            _StartTime = UnityEngine.Time.time;
        }

        protected override NodeState OnUpdate(BehaviourTreeRunner runner)
        {
            if (IsTimeOver())
            {
                return NodeState.Success;
            }

            return NodeState.Running;
        }

        private bool IsTimeOver() =>
            UnityEngine.Time.time - _StartTime >= _Time;
    }
}