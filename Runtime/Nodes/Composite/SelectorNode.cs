namespace MoshitinEncoded.BehaviourTree
{
    [NodeMenu("Composite/Selector")]
    public class SelectorNode : CompositeNode
    {
        protected override void OnStart()
        {
            
        }

        protected override void OnStop()
        {
            
        }

        protected override State OnUpdate()
        {
            foreach (var child in children)
            {
                var childState = child.Update();
                if (childState != State.Failure)
                {
                    return childState;
                }
            }

            return State.Failure;
        }
    }
}