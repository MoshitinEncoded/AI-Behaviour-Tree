using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Composite/Sequencer")]
    public class SequencerNode : CompositeNode
    {
        [SerializeField, Tooltip("Whether to update all nodes in the same frame.")]
        private bool _UpdateInTheSameFrame = false;

        private int current;
        protected override void OnStart(BehaviourTreeRunner runner)
        {
            current = 0;
        }

        protected override void OnStop(BehaviourTreeRunner runner)
        {
            
        }

        protected override NodeState OnUpdate(BehaviourTreeRunner runner)
        {
            do
            {
                var child = Children[current];
                switch (child.UpdateNode(runner))
                {
                    case NodeState.Running:
                        return NodeState.Running;
                    case NodeState.Failure:
                        return NodeState.Failure;
                    case NodeState.Success:
                        current++;
                        break;
                }
            } while (_UpdateInTheSameFrame && current < Children.Count);

            return current == Children.Count ? NodeState.Success : NodeState.Running;
        }
    }
}