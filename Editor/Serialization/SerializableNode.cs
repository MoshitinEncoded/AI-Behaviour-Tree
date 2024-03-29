using MoshitinEncoded.AI.BehaviourTreeLib;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    [System.Serializable]
    internal class SerializableNode
    {
        public SerializableNode() {}

        public SerializableNode(Node node, string[] childGuids)
        {
            Node = node;
            ChildGuids = childGuids;
        }

        public Node Node;
        public string[] ChildGuids;
    }
}