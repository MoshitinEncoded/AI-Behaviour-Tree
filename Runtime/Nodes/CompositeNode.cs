namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    public abstract class CompositeNode : NodeBehaviour
    {
        public Node[] Children => Node.Children;
    }
}