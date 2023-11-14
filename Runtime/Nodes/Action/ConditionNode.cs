using UnityEngine;

namespace MoshitinEncoded.AIBehaviourTree
{
    public abstract class ConditionNode : ActionNode
    {
        [SerializeField, Tooltip("Whether to invert the condition result.")]
        private bool _Invert;
        protected override NodeState OnUpdate()
        {
            var result = OnEvaluate();
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

        protected abstract NodeState OnEvaluate();
    }
}
