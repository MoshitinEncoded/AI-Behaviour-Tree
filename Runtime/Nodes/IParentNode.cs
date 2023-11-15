using System.Collections.Generic;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    public interface IParentNode
    {
        void AddChild(Node child);
        List<Node> GetChildren();
        void ClearChildren();
    }
}
