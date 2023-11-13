using System.Collections.Generic;
using UnityEngine;
using MoshitinEncoded.BehaviourTree;

namespace MoshitinEncoded.Editor.BehaviourTree
{
    internal class DeserializedSelection
    {
        public Rect SelectionRect;
        public Dictionary<Node, List<Node>> NodeChildsDict;
    }
}
