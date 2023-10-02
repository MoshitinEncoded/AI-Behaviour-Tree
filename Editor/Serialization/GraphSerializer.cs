using System.Collections.Generic;
using System.Linq;
using Unity.Serialization.Json;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using MoshitinEncoded.BehaviourTree;
using Node = MoshitinEncoded.BehaviourTree.Node;

namespace MoshitinEncoded.Editor.BehaviourTree
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
                SerializableNodes = GetSerializableNodes(elements)
            };

            return ToJson(serializableSelection);
        }

        public static GraphSelection UnserializeSelection(string json)
        {
            var serializableSelection = FromJson(json);
            var serializableNodes = serializableSelection.SerializableNodes;

            // Add node childs
            foreach (var serializableNode in serializableNodes)
            {
                if (serializableNode.Node is IParentNode parentNode)
                {
                    foreach (var childGuid in serializableNode.ChildGuids)
                    {
                        var childNode = FindChild(serializableNodes, childGuid).Node;
                        parentNode.AddChild(childNode);
                    }
                }
            }

            var graphSelection = new GraphSelection()
            {
                SelectionRect = serializableSelection.SelectionRect,
                Nodes = ExtractNodes(serializableNodes)
            };

            return graphSelection;
        }

        private static IEnumerable<Node> ExtractNodes(SerializableNode[] serializableNodes)
        {
            var nodes = new List<Node>();
            foreach (var serializableNode in serializableNodes)
            {
                var node = serializableNode.Node;
                node.name = node.GetType().Name;
                
                nodes.Add(node);
            }

            return nodes;
        }

        private static SerializableNode FindChild(SerializableNode[] serializableNodes, string childGuid) =>
            serializableNodes.First(n => n.Node.guid == childGuid);

        private static SerializableNode[] GetSerializableNodes(IEnumerable<GraphElement> elements)
        {
            var nodeViews = GetElementsTypeOf<NodeView>(elements);
            var edges = GetElementsTypeOf<Edge>(elements);
            var serializableNodes = CreateSerializableNodes(nodeViews, edges);

            return serializableNodes.ToArray();
        }

        private static List<SerializableNode> CreateSerializableNodes(List<NodeView> nodeViews, List<Edge> edges)
        {
            var nodeCopies = CreateNodeCopies(nodeViews);

            var serializableNodes = new List<SerializableNode>();

            // Creo las copias que voy a serializar con sus respectivos hijos como GUID
            foreach (var nodeCopy in nodeCopies)
            {
                var childGuids = new List<string>();
                var connectedEdges = edges.Where(edge => edge.output.node == nodeCopy.Key);
                foreach (var connectedEdge in connectedEdges)
                {
                    var childNodeView = (NodeView)connectedEdge.input.node;
                    var childCopy = nodeCopies[childNodeView];
                    childGuids.Add(childCopy.guid);
                }

                serializableNodes.Add(new SerializableNode(nodeCopy.Value, childGuids.ToArray()));
            }

            return serializableNodes;
        }

        private static Dictionary<NodeView, Node> CreateNodeCopies(List<NodeView> nodeViews)
        {
            var copies = new Dictionary<NodeView, Node>();
            foreach (var originalNodeView in nodeViews)
            {
                var nodeCopy = originalNodeView.Node.Clone(withChildren: false);
                nodeCopy.guid = GUID.Generate().ToString();
                nodeCopy.name = nodeCopy.name.Remove(nodeCopy.name.IndexOf('('));

                copies.Add(
                    key: originalNodeView,
                    value: nodeCopy
                );
            }

            return copies;
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
