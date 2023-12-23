namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Composite/Selector", "Runs its children until one succeeds or all fails.")]
    public class SelectorNode : CompositeNode
    {
        protected override NodeState Run(BehaviourTreeRunner runner)
        {
            foreach (var child in Children)
            {
                if (child == null) continue;
                
                var childState = child.RunBehaviour(runner);
                if (childState != NodeState.Failure)
                {
                    return childState;
                }
            }

            return NodeState.Failure;
        }
    }
}