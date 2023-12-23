namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Decorator/Invert")]
    [NodeDescription("Returns the inverted state of its child (except \"running\").")]
    public class InvertNode : DecoratorNode
    {
        protected override NodeState Run(BehaviourTreeRunner runner)
        {
            var state = Child.RunBehaviour(runner);
            switch (state)
            {
                case NodeState.Failure:
                    state = NodeState.Success;
                    break;
                case NodeState.Success:
                    state = NodeState.Failure;
                    break;
            }

            return state;
        }
    }
}
