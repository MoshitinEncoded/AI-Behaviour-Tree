using System;

namespace MoshitinEncoded.BehaviourTree {
    public class AddParameterMenuAttribute : Attribute
    {
        public string MenuPath = string.Empty;
        public AddParameterMenuAttribute(string menuPath)
        {
            MenuPath = menuPath;
        }
    }
}
