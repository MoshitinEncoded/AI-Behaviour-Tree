using System;

namespace MoshitinEncoded.AIBehaviourTree {
    public class AddParameterMenuAttribute : Attribute
    {
        public string MenuPath = string.Empty;
        public AddParameterMenuAttribute(string menuPath)
        {
            MenuPath = menuPath;
        }
    }
}
