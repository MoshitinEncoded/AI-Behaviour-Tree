using MoshitinEncoded.AI.BehaviourTreeLib;

using Unity.Serialization.Json;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    public class NodeBehaviourJsonAdapter : IJsonAdapter<NodeBehaviour>
    {
        public NodeBehaviour Deserialize(in JsonDeserializationContext<NodeBehaviour> context)
            {
                var parameters = new JsonSerializationParameters()
                {
                    DisableRootAdapters = true
                };
        
                return JsonSerialization.FromJson<NodeBehaviour>(context.SerializedValue, parameters);
            }
        
            public void Serialize(in JsonSerializationContext<NodeBehaviour> context, NodeBehaviour value)
            {
                var parameters = new JsonSerializationParameters()
                {
                    DisableRootAdapters = true
                };
    
                context.Writer.WriteValueLiteral(JsonSerialization.ToJson(value, parameters));
            }
    }
}
