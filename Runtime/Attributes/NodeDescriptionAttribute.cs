using System;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    public class NodeDescriptionAttribute : Attribute
    {
        private readonly string _Description;
    
        public string Description => _Description;
    
        public NodeDescriptionAttribute(string description)
        {
            _Description = description;
        }
    }
}
