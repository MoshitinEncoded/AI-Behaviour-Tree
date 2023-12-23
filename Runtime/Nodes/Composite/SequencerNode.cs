using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Composite/Sequencer")]
    [NodeDescription("Runs its children until one fails or all succeeds.")]
    public class SequencerNode : CompositeNode
    {
        [SerializeField, Tooltip("Whether to update all nodes in the same frame.")]
        private bool _UpdateInTheSameFrame = false;

        private int current;
        protected override void OnStart(BehaviourTreeRunner runner)
        {
            current = 0;
        }

        protected override NodeState Run(BehaviourTreeRunner runner)
        {
            if (Children.Length < 1)
            {
                return NodeState.Success;
            }

            do
            {
                var child = Children[current];
                if (child == null)
                {
                    current++;
                    continue;
                }

                switch (child.RunBehaviour(runner))
                {
                    case NodeState.Running:
                        return NodeState.Running;
                    case NodeState.Failure:
                        return NodeState.Failure;
                    case NodeState.Success:
                        current++;
                        break;
                }
            } while (_UpdateInTheSameFrame && current < Children.Length);

            return current == Children.Length ? NodeState.Success : NodeState.Running;
        }
    }
}