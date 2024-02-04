using System.Collections.Generic;

using MoshitinEncoded.AI.BehaviourTreeLib;

using Unity.Serialization.Json;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    public class NodeJsonAdapter : IJsonAdapter<Node>
    {
        public Node Deserialize(in JsonDeserializationContext<Node> context)
        {
            var parameters = new JsonSerializationParameters()
            {
                DisableRootAdapters = true,
                UserDefinedAdapters = new List<IJsonAdapter>() { new NodeBehaviourJsonAdapter() }
            };
    
            return JsonSerialization.FromJson<Node>(context.SerializedValue, parameters);
        }
    
        public void Serialize(in JsonSerializationContext<Node> context, Node value)
        {
            var parameters = new JsonSerializationParameters()
            {
                DisableRootAdapters = true,
                UserDefinedAdapters = new List<IJsonAdapter>() { new NodeBehaviourJsonAdapter() }
            };
    
            context.Writer.WriteValueLiteral(JsonSerialization.ToJson(value, parameters));
        }
    }
}
