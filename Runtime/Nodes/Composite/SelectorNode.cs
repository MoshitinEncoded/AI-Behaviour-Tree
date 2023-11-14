namespace MoshitinEncoded.AIBehaviourTree
{
    [CreateNodeMenu("Composite/Selector")]
    public class SelectorNode : CompositeNode
    {
        protected override void OnStart()
        {
            
        }

        protected override void OnStop()
        {
            
        }

        protected override NodeState OnUpdate()
        {
            foreach (var child in Children)
            {
                var childState = child.Update();
                if (childState != NodeState.Failure)
                {
                    return childState;
                }
            }

            return NodeState.Failure;
        }
    }
}