namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    public abstract class DecoratorNode : NodeBehaviour
    {
        public Node Child => Node.Children.Length > 0 ? Node.Children[0] : null;
    }
}