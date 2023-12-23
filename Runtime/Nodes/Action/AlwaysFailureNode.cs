namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Task/Condition/Always Failure")]
    [NodeDescription("Returns failure, always.")]
    public class AlwaysFailureNode : TaskNode
    {
        protected override NodeState Run(BehaviourTreeRunner runner)
        {
            return NodeState.Failure;
        }
    }
}
