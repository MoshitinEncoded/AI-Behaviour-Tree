using System.Collections.Generic;

namespace MoshitinEncoded.AIBehaviourTree
{
    public interface IParentNode
    {
        void AddChild(Node child);
        List<Node> GetChildren();
        void ClearChildren();
    }
}
