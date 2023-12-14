using MoshitinEncoded.AI.BehaviourTreeLib;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    [System.Serializable]
    internal class SerializableNode
    {
        public SerializableNode() {}

        public SerializableNode(NodeBehaviour node, string[] childGuids)
        {
            Node = node;
            ChildGuids = childGuids;
        }

        public NodeBehaviour Node;
        public string[] ChildGuids;
    }
}