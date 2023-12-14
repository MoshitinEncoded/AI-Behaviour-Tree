using System.Collections.Generic;
using UnityEngine;
using MoshitinEncoded.AI.BehaviourTreeLib;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class GraphSelection
    {
        public Rect SelectionRect;
        public Dictionary<NodeBehaviour, List<NodeBehaviour>> ChildsDict;
    }
}
