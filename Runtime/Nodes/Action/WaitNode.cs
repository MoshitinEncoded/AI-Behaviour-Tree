using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Action/Wait")]
    public class WaitNode : ActionNode
    {
        public float duration = 1f;
        float startTime;

        protected override void OnStart(BehaviourTreeRunner runner)
        {
            startTime = Time.time;
        }

        protected override NodeState OnUpdate(BehaviourTreeRunner runner)
        {
            if (TimeOver())
            {
                return NodeState.Success;
            }

            return NodeState.Running;
        }

        protected override void OnStop(BehaviourTreeRunner runner)
        {
            
        }

        private bool TimeOver() =>
            Time.time - startTime >= duration;
    }
}