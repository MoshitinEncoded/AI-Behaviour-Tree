namespace MoshitinEncoded.AIBehaviourTree
{
    [CreateNodeMenu("Composite/Selector")]
    public class SelectorNode : CompositeNode
    {
        protected override void OnStart(BehaviourTreeRunner runner)
        {
            
        }

        protected override void OnStop(BehaviourTreeRunner runner)
        {
            
        }

        protected override NodeState OnUpdate(BehaviourTreeRunner runner)
        {
            foreach (var child in Children)
            {
                var childState = child.Update(runner);
                if (childState != NodeState.Failure)
                {
                    return childState;
                }
            }

            return NodeState.Failure;
        }
    }
}