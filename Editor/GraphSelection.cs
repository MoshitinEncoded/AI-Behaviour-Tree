using System.Collections.Generic;
using UnityEngine;
using MoshitinEncoded.AIBehaviourTree;

namespace MoshitinEncoded.Editor.AIBehaviourTree
{
    internal class DeserializedSelection
    {
        public Rect SelectionRect;
        public Dictionary<Node, List<Node>> NodeChildsDict;
    }
}
