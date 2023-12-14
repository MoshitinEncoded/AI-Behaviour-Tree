using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    public class Node : ScriptableObject
    {

#if UNITY_EDITOR
        [SerializeField] private string _Title;
        [SerializeField, HideInInspector] private string _Guid;
        [SerializeField, HideInInspector] private Vector2 _Position;
#endif

        [SerializeField, HideInInspector] private NodeBehaviour _Behaviour;
        [SerializeField, HideInInspector] private Node[] _Children;

        private bool _Initialized = false;
        private bool _Started = false;

        public NodeState State { get; private set; } = NodeState.Running;

        public float StartTime { get; private set; } = float.MinValue;

        public float LastRunTime { get; private set; } = float.MinValue;

        public NodeBehaviour Behaviour => _Behaviour;

        public Node[] Children => _Children;

        public NodeState RunBehaviour(BehaviourTreeRunner runner)
        {
            if (!_Behaviour)
            {
                return NodeState.Failure;
            }

            if (!_Initialized)
            {
                Initialize(runner);
            }

            if (!_Started)
            {
                StartBehaviour(runner);
            }

            State = _Behaviour.RunBehaviour(runner);

            if (State != NodeState.Running)
            {
                StopBehaviour(runner);
            }

            LastRunTime = Time.time;

            return State;
        }

        /// <summary>
        /// Clones this node along with his hierarchy.
        /// </summary>
        /// <returns></returns>
        internal Node Clone() => Clone(withHierarchy: true);

        /// <summary>
        /// Clones this node with no children.
        /// </summary>
        /// <returns></returns>
        internal Node CloneAlone() => Clone(withHierarchy: false);

        private void Initialize(BehaviourTreeRunner runner)
        {
            _Behaviour.Initialize(runner);
            _Initialized = true;
        }

        private void StartBehaviour(BehaviourTreeRunner runner)
        {
            _Behaviour.StartBehaviour(runner);
            _Started = true;
            StartTime = Time.time;
        }

        private void StopBehaviour(BehaviourTreeRunner runner)
        {
            _Behaviour.StopBehaviour(runner);
            _Started = false;
        }

        private Node Clone(bool withHierarchy)
        {
            var clone = Instantiate(this);

            clone._Behaviour = _Behaviour.Clone(node: clone);
            clone._Children = withHierarchy ? CloneChildren() : new Node[0];

            return clone;
        }

        private Node[] CloneChildren()
        {
            var childrenClones = new Node[_Children.Length];
            for (int i = 0; i < _Children.Length; i++)
            {
                childrenClones[i] = _Children[i].Clone();
            }

            return childrenClones;
        }
    }
}
