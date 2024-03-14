using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Composite/Sequencer")]
    [Tooltip("Runs its children until one fails or all succeeds.")]
    public class SequencerNode : CompositeNode
    {
        private int _RunningChildIndex;

        protected override void OnStart(BehaviourTreeRunner runner)
        {
            _RunningChildIndex = 0;
        }

        protected override NodeState Run(BehaviourTreeRunner runner)
        {
            if (Children.Length < 1)
            {
                return NodeState.Failure;
            }

            var child = Children[_RunningChildIndex];
            var childState = child.RunBehaviour(runner);

            return childState switch
            {
                NodeState.Running => NodeState.Running,
                NodeState.Failure => NodeState.Failure,
                NodeState.Success => GoToNextChild(),
                _ => LogInvalidNodeStateException(runner),
            };
        }

        private NodeState GoToNextChild()
        {
            _RunningChildIndex++;
            return _RunningChildIndex == Children.Length ? NodeState.Success : NodeState.Running;
        }

        private NodeState LogInvalidNodeStateException(BehaviourTreeRunner runner)
        {
            Debug.LogException(
                new System.InvalidOperationException($"{Node.name} Exception: child's returned state is an invalid enum value."),
                runner);
            return NodeState.Failure;
        }
    }
}