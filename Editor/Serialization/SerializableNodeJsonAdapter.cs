using MoshitinEncoded.AI.BehaviourTreeLib;
using MoshitinEncoded.Editor.AI.BehaviourTreeLib;
using Unity.Serialization.Json;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class SerializableNodeJsonAdapter : IJsonAdapter<SerializableNode>
    {
        public SerializableNode Deserialize(in JsonDeserializationContext<SerializableNode> context)
        {
            var serializedNode = context.SerializedValue.GetValue("Node");
            var nodeParams = new JsonSerializationParameters()
            {
                DisableRootAdapters = true
            };

            var node = JsonSerialization.FromJson<Node>(serializedNode, nodeParams);
            var childGuids = JsonSerialization.FromJson<string[]>(context.SerializedValue.GetValue("ChildGuids"));

            return new SerializableNode(node, childGuids);
        }

        public void Serialize(in JsonSerializationContext<SerializableNode> context, SerializableNode value)
        {
            context.Writer.WriteBeginObject();

            var nodeParams = new JsonSerializationParameters()
            {
                DisableRootAdapters = true
            };

            context.Writer.WriteKeyValueLiteral("Node", JsonSerialization.ToJson(value.Node, nodeParams));

            context.Writer.WriteBeginArray("ChildGuids");
            foreach (var nodeGuid in value.ChildGuids)
            {
                context.Writer.WriteValue(nodeGuid);
            }
            context.Writer.WriteEndArray();

            context.Writer.WriteEndObject();
        }
    }
}