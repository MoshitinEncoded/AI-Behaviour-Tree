using MoshitinEncoded.GraphTools;

using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Task/Debug/Log Parameter")]
    [Tooltip("Logs a parameter value to the Unity console.")]
    public class LogParameterNode : TaskNode
    {
        [Space]
        [SerializeField] private string _ParameterName;

        private BlackboardParameter _Parameter;

        protected override void OnStart(BehaviourTreeRunner runner)
        {
            if (_Parameter == null || _Parameter.ParameterName != _ParameterName)
            {
                _Parameter = runner.GetParameter(_ParameterName);
            }
        }

        protected override NodeState Run(BehaviourTreeRunner runner)
        {
            if (_Parameter == null)
            {
                return NodeState.Failure;
            }

            Debug.Log(_ParameterName + " Parameter: " + _Parameter.BoxedValue, runner);
            return NodeState.Success;
        }
    }
}
