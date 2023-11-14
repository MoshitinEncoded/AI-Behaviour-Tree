namespace MoshitinEncoded.AIBehaviourTree
{
    [CreateNodeMenu("Decorator/Repeater")]
    public class RepeatNode : DecoratorNode
    {
        protected override void OnStart(BehaviourTreeRunner runner)
        {
            
        }

        protected override NodeState OnUpdate(BehaviourTreeRunner runner)
        {
            Child.Update(runner);
            return NodeState.Running;
        }

        protected override void OnStop(BehaviourTreeRunner runner)
        {
            
        }
    }
}