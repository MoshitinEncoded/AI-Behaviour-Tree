using MoshitinEncoded.GraphTools;

using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Task/Debug/Log Parameter")]
    public class LogParameterNode : TaskNode
    {
        [Space]
        [SerializeField] private string _ParameterName;

        private BlackboardParameter _Parameter;

        protected override void OnInitialize(BehaviourTreeRunner runner)
        {
            _Parameter = runner.GetParameterByRef(_ParameterName);
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
