namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Decorator/Repeater", "Runs its child forever.")]
    public class RepeatNode : DecoratorNode
    {
        protected override NodeState Run(BehaviourTreeRunner runner)
        {
            Child.RunBehaviour(runner);
            return NodeState.Running;
        }
    }
}