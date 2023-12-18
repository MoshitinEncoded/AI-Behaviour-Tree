using System.Collections.Generic;

using Unity.Serialization.Json;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class SerializableNodeJsonAdapter : IJsonAdapter<SerializableNode>
    {
        public SerializableNode Deserialize(in JsonDeserializationContext<SerializableNode> context)
        {
            var parameters = new JsonSerializationParameters()
            {
                DisableRootAdapters = true,
                UserDefinedAdapters = new List<IJsonAdapter>() { new NodeJsonAdapter() }
            };

            return JsonSerialization.FromJson<SerializableNode>(context.SerializedValue, parameters);
        }

        public void Serialize(in JsonSerializationContext<SerializableNode> context, SerializableNode value)
        {
            var parameters = new JsonSerializationParameters()
            {
                DisableRootAdapters = true,
                UserDefinedAdapters = new List<IJsonAdapter>() { new NodeJsonAdapter() }
            };

            context.Writer.WriteValueLiteral(JsonSerialization.ToJson(value, parameters));
        }
    }
}