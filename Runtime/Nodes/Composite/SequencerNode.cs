using UnityEngine;

namespace MoshitinEncoded.BehaviourTree
{
    [CreateNodeMenu("Composite/Sequencer")]
    public class SequencerNode : CompositeNode
    {
        [SerializeField, Tooltip("Whether to update all nodes in the same frame.")]
        private bool _UpdateInTheSameFrame = false;

        private int current;
        protected override void OnStart()
        {
            current = 0;
        }

        protected override void OnStop()
        {
            
        }

        protected override NodeState OnUpdate()
        {
            do
            {
                var child = Children[current];
                switch (child.Update())
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