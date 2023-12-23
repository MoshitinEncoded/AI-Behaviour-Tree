namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    public class RootNode : NodeBehaviour
    {
        public Node Child => Node.Children.Length > 0 ? Node.Children[0] : null;

        protected override NodeState Run(BehaviourTreeRunner runner) =>
            (!Child) ? NodeState.Failure : Child.RunBehaviour(runner);
    }
}