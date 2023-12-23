namespace MoshitinEncoded.AI.BehaviourTreeLib {
    public class CreateNodeMenuAttribute : System.Attribute
    {
        private readonly string _Path;
        private readonly string _Description = "";
        
        public string Path => _Path;

        public string Description => _Description;

        public CreateNodeMenuAttribute(string path)
        {
            _Path = path;
        }

        public CreateNodeMenuAttribute(string path, string description)
        {
            _Path = path;
            _Description = description;
        }
    }
}
