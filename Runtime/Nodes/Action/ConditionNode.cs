using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    public abstract class ConditionNode : ActionNode
    {
        [SerializeField, Tooltip("Whether to invert the condition result.")]
        private bool _Invert;
        protected override NodeState OnUpdate(BehaviourTreeRunner runner)
        {
            var result = OnEvaluate(runner);
            if (_Invert)
            {
                result = InvertState(result);
            }

            return result;
        }

        private static NodeState InvertState(NodeState state) => state switch
        {
            NodeState.Running => NodeState.Running,
            NodeState.Failure => NodeState.Success,
            NodeState.Success => NodeState.Failure,
            _ => throw new System.NotImplementedException(),
        };

        protected abstract NodeState OnEvaluate(BehaviourTreeRunner runner);
    }
}
