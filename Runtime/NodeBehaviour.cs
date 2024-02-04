using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    public abstract class NodeBehaviour : ScriptableObject
    {
        [SerializeField, HideInInspector] private Node _Node;

        public Node Node => _Node;

        public NodeState State => _Node.State;

        public float StartTime => _Node.StartTime;

        public float LastUpdateTime => _Node.LastRunTime;

        internal virtual NodeBehaviour Clone(Node node)
        {
            var clone = Instantiate(this);
            clone._Node = node;
            
            return clone;
        }

        internal void Initialize(BehaviourTreeRunner runner)
        {
            OnInitialize(runner);
        }

        internal void StartBehaviour(BehaviourTreeRunner runner)
        {
            OnStart(runner);
        }

        internal NodeState RunBehaviour(BehaviourTreeRunner runner)
        {
            return Run(runner);
        }

        internal void StopBehaviour(BehaviourTreeRunner runner)
        {
            OnStop(runner);
        }

        /// <summary>
        /// Called the first time this node runs.
        /// </summary>
        /// <param name="runner"></param>
        protected virtual void OnInitialize(BehaviourTreeRunner runner) { }

        /// <summary>
        /// Called when this node starts running.
        /// </summary>
        /// <param name="runner"> Runner that started the update. </param>
        protected virtual void OnStart(BehaviourTreeRunner runner) { }

        /// <summary>
        /// Called each time this node is runned.
        /// </summary>
        /// <param name="runner"> Runner that started the update. </param>
        /// <returns></returns>
        protected abstract NodeState Run(BehaviourTreeRunner runner);

        /// <summary>
        /// Called when this node stops running.
        /// </summary>
        /// <param name="runner"> Runner that started the update. </param>
        protected virtual void OnStop(BehaviourTreeRunner runner) { }
    }

    public enum NodeState
    {
        Running,
        Failure,
        Success
    }
}