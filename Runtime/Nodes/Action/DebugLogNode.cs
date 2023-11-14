using UnityEngine;

namespace MoshitinEncoded.AIBehaviourTree
{
    [CreateNodeMenu("Action/Debug Log")]
    public class DebugLogNode : ActionNode
    {
        public string message;
        [SerializeField] private string _timeId;
        protected override void OnStart(BehaviourTreeRunner runner)
        {
            Debug.Log($"OnStart{message}");
        }

        protected override NodeState OnUpdate(BehaviourTreeRunner runner)
        {
            Debug.Log($"OnUpdate{message}");
            return NodeState.Success;
        }

        protected override void OnStop(BehaviourTreeRunner runner)
        {
            Debug.Log($"OnStop{message}");
        }
    }
}