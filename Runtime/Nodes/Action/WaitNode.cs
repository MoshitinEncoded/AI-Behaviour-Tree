using UnityEngine;

namespace MoshitinEncoded.BehaviourTree
{
    [CreateNodeMenu("Action/Wait")]
    public class WaitNode : ActionNode
    {
        public float duration = 1f;
        float startTime;

        protected override void OnStart()
        {
            startTime = Time.time;
        }

        protected override NodeState OnUpdate()
        {
            if (TimeOver())
            {
                return NodeState.Success;
            }

            return NodeState.Running;
        }

        protected override void OnStop()
        {
            
        }

        private bool TimeOver() =>
            Time.time - startTime >= duration;
    }
}