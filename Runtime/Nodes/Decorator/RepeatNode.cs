namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Decorator/Repeater")]
    public class RepeatNode : DecoratorNode
    {
        protected override void OnStart(BehaviourTreeRunner runner)
        {
            
        }

        protected override NodeState OnUpdate(BehaviourTreeRunner runner)
        {
            Child.UpdateNode(runner);
            return NodeState.Running;
        }

        protected override void OnStop(BehaviourTreeRunner runner)
        {
            
        }
    }
}