using System.Collections.Generic;
using UnityEngine;
using MoshitinEncoded.AI.BehaviourTreeLib;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class DeserializedSelection
    {
        public Rect SelectionRect;
        public Dictionary<Node, List<Node>> NodeChildsDict;
    }
}
