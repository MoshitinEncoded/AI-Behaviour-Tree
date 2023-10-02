using System.Collections.Generic;

namespace MoshitinEncoded.BehaviourTree
{
    public interface IParentNode
    {
        void AddChild(Node child);
        List<Node> GetChildren();
    }
}
