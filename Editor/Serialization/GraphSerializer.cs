using System.Collections.Generic;
using System.Linq;

using MoshitinEncoded.AI.BehaviourTreeLib;

using Unity.Serialization.Json;

using UnityEditor;
using UnityEditor.Experimental.GraphView;

using UnityEngine;

using Node = MoshitinEncoded.AI.BehaviourTreeLib.Node;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal static class GraphSerializer
    {
        private static JsonSerializationParameters _SerializationParameters = new()
        {
            UserDefinedAdapters = new List<IJsonAdapter>() { new SerializableNodeJsonAdapter() }
        };

        public static string SerializeSelection(Rect selectionRect, IEnumerable<GraphElement> elements)
        {
            var serializableSelection = new SerializableGraphSelection()
            {
                SelectionRect = selectionRect,
                SerializableNodes = GetSerializableNodes(elements)
            };

            return ToJson(serializableSelection);
        }

        public static GraphSelection UnserializeSelection(string json)
        {
            var serializableSelection = FromJson(json);
            var serializableNodes = serializableSelection.SerializableNodes;
            var nodeChildsDict = new Dictionary<Node, List<Node>>();

            // Add node childs
            foreach (var serializableNode in serializableNodes)
            {
                nodeChildsDict.Add(serializableNode.Node, new List<Node>());
                foreach (var childGuid in serializableNode.ChildGuids)
                {
                    var childNode = FindChild(serializableNodes, childGuid).Node;
                    nodeChildsDict[serializableNode.Node].Add(childNode);
                }
            }

            var graphSelection = new GraphSelection()
            {
                SelectionRect = serializableSelection.SelectionRect,
                ChildsDict = nodeChildsDict
            };

            return graphSelection;
        }

        private static SerializableNode FindChild(SerializableNode[] serializableNodes, string childGuid)
        {
            return serializableNodes.First(n => new SerializedObject(n.Node).FindProperty("_Guid").stringValue == childGuid);
        }

        private static SerializableNode[] GetSerializableNodes(IEnumerable<GraphElement> elements)
        {
            var nodeChildsDict = new Dictionary<Node, List<string>>();

            foreach (var element in elements)
            {
                if (element is NodeView nodeView)
                {
                    nodeChildsDict.Add(nodeView.Node, new List<string>());
                }
            }

            foreach (var element in elements)
            {
                if (element is Edge edge)
                {
                    AddChildIfValid(nodeChildsDict, edge);
                }
            }

            var serializableNodes = CreateSerializableNodes(nodeChildsDict);

            return serializableNodes.ToArray();
        }

        private static void AddChildIfValid(Dictionary<Node, List<string>> nodeChildsDict, Edge edge)
        {
            var childNode = edge.input.node as NodeView;
            var parentNode = edge.output.node as NodeView;
            if (!nodeChildsDict.ContainsKey(childNode.Node) ||
                !nodeChildsDict.ContainsKey(parentNode.Node))
                return;

            var childGuid = new SerializedObject(childNode.Node).FindProperty("_Guid").stringValue;
            nodeChildsDict[parentNode.Node].Add(childGuid);
        }

        private static List<SerializableNode> CreateSerializableNodes(Dictionary<Node, List<string>> nodeChildsDict)
        {
            var serializableNodes = new List<SerializableNode>();

            foreach (var nodeChildsPair in nodeChildsDict)
            {
                serializableNodes.Add(new SerializableNode(
                    nodeChildsPair.Key,
                    nodeChildsPair.Value.ToArray()
                ));
            }

            return serializableNodes;
        }

        private static List<TElement> GetElementsTypeOf<TElement>(IEnumerable<GraphElement> elements)
            where TElement : GraphElement
        {
            var elementsToReturn = new List<TElement>();
            foreach (var element in elements)
            {
                if (element is TElement tElement)
                {
                    elementsToReturn.Add(tElement);
                }
            }

            return elementsToReturn;
        }

        private static string ToJson(SerializableGraphSelection serializableSelection) =>
            JsonSerialization.ToJson(serializableSelection, _SerializationParameters);

        private static SerializableGraphSelection FromJson(string data) =>
            JsonSerialization.FromJson<SerializableGraphSelection>(data, _SerializationParameters);
    }

    public class GraphElementGrouper
    {
        public GraphElementGrouper(IEnumerable<GraphElement> elements)
        {
            GroupElements(elements);
        }

        public void GroupElements(IEnumerable<GraphElement> elements)
        {
            foreach (var element in elements)
            {
                switch (element)
                {
                    case NodeView nodeView:
                        NodeViews.Add(nodeView);
                        break;
                    case Edge edge:
                        Edges.Add(edge);
                        break;
                }
            }
        }

        internal List<NodeView> NodeViews = new();
        public List<Edge> Edges = new();
    }
}
