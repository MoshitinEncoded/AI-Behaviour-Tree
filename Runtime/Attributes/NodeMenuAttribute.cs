namespace MoshitinEncoded.AI.BehaviourTreeLib {
    public class CreateNodeMenuAttribute : System.Attribute
    {
        public string Path;
        public CreateNodeMenuAttribute(string path)
        {
            Path = path;
        }
    }
}
