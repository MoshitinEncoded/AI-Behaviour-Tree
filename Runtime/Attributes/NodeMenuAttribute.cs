namespace MoshitinEncoded.BehaviourTree {
    public class NodeMenuAttribute : System.Attribute
    {
        public string Path;
        public NodeMenuAttribute(string path)
        {
            Path = path;
        }
    }
}
