using UnityEngine;

namespace MoshitinEncoded.BehaviourTree
{
    [NodeMenu("Action/Debug Log")]
    public class DebugLogNode : ActionNode
    {
        public string message;
        [SerializeField] private string _timeId;
        protected override void OnStart()
        {
            Debug.Log($"OnStart{message}");
        }

        protected override State OnUpdate()
        {
            Debug.Log($"OnUpdate{message}");
            return State.Success;
        }

        protected override void OnStop()
        {
            Debug.Log($"OnStop{message}");
        }
    }
}