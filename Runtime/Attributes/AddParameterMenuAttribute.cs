using System;

namespace MoshitinEncoded.AI.BehaviourTreeLib {
    public class AddParameterMenuAttribute : Attribute
    {
        public string MenuPath = string.Empty;
        public AddParameterMenuAttribute(string menuPath)
        {
            MenuPath = menuPath;
        }
    }
}
