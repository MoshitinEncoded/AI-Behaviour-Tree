using UnityEngine;

namespace MoshitinEncoded.BehaviourTree
{
    [CreateNodeMenu("Action/Debug Log")]
    public class DebugLogNode : ActionNode
    {
        public string message;
        [SerializeField] private string _timeId;
        protected override void OnStart()
        {
            Debug.Log($"OnStart{message}");
        }

        protected override NodeState OnUpdate()
        {
            Debug.Log($"OnUpdate{message}");
            return NodeState.Success;
        }

        protected override void OnStop()
        {
            Debug.Log($"OnStop{message}");
        }
    }
}