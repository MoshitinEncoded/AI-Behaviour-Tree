using System.Collections.Generic;
using System.Linq;
using Unity.Serialization.Json;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using MoshitinEncoded.AI.BehaviourTreeLib;
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
            var serializableSelection = new SerializableNodeSelection()
            {
                SelectionRect = selectionRect,
                SerializedNodes = GetSerializableNodes(elements)
            };

            return ToJson(serializableSelection);
        }

        public static DeserializedSelection UnserializeSelection(string json)
        {
            var serializableSelection = FromJson(json);
            var serializedNodes = serializableSelection.SerializedNodes;
            var nodeChildsDict = new Dictionary<Node, List<Node>>();

            // Add node childs
            foreach (var serializedNode in serializedNodes)
            {
                nodeChildsDict.Add(serializedNode.Node, new List<Node>());
                foreach (var childGuid in serializedNode.ChildGuids)
                {
                    var childNode = FindChild(serializedNodes, childGuid).Node;
                    nodeChildsDict[serializedNode.Node].Add(childNode);
                }
            }

            var graphSelection = new DeserializedSelection()
            {
                SelectionRect = serializableSelection.SelectionRect,
                NodeChildsDict = nodeChildsDict
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
            var nodeViews = GetElementsTypeOf<NodeView>(elements);
            var edges = GetElementsTypeOf<Edge>(elements);
            foreach (var nodeView in nodeViews)
            {
                if (nodeView.Node is RootNode) continue;
                nodeChildsDict.Add(nodeView.Node, new List<string>());
            }

            foreach (var edge in edges)
            {
                var inputNode = edge.input.node as NodeView;
                var outputNode = edge.output.node as NodeView;
                if (!nodeChildsDict.ContainsKey(inputNode.Node) || 
                    !nodeChildsDict.ContainsKey(outputNode.Node))
                        continue;
                
                var childGuid = new SerializedObject(inputNode.Node).FindProperty("_Guid").stringValue;
                nodeChildsDict[outputNode.Node].Add(childGuid);
            }

            var serializableNodes = CreateSerializableNodes(nodeChildsDict);

            return serializableNodes.ToArray();
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

        private static string ToJson(SerializableNodeSelection serializableSelection) =>
            JsonSerialization.ToJson(serializableSelection, _SerializationParameters);

        private static SerializableNodeSelection FromJson(string data) =>
            JsonSerialization.FromJson<SerializableNodeSelection>(data, _SerializationParameters);
    }
}
