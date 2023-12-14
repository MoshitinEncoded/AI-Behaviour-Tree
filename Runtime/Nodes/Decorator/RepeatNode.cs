namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Decorator/Repeater")]
    public class RepeatNode : DecoratorNode
    {
        protected override NodeState Run(BehaviourTreeRunner runner)
        {
            Child.RunBehaviour(runner);
            return NodeState.Running;
        }
    }
}