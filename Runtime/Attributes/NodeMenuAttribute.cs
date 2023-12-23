namespace MoshitinEncoded.AI.BehaviourTreeLib {
    public class CreateNodeMenuAttribute : System.Attribute
    {
        private readonly string _Path;
        
        public string Path => _Path;

        public CreateNodeMenuAttribute(string path)
        {
            _Path = path;
        }
    }
}
