namespace MoshitinEncoded.AIBehaviourTree
{
    [CreateNodeMenu("Decorator/Repeater")]
    public class RepeatNode : DecoratorNode
    {
        protected override void OnStart()
        {
            
        }

        protected override NodeState OnUpdate()
        {
            Child.Update();
            return NodeState.Running;
        }

        protected override void OnStop()
        {
            
        }
    }
}