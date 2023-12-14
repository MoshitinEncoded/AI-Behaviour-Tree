using System.Collections.Generic;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    public interface IParentNode
    {
        void AddChild(NodeBehaviour child);
        void ClearChildren();
    }
}
