using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    public abstract class Node : ScriptableObject
    {

#if UNITY_EDITOR
        [SerializeField] private string _Title;
        [SerializeField, HideInInspector] private string _Guid;
        [SerializeField, HideInInspector] private Vector2 _Position;
#endif

        private bool _Initialized = false;

        [HideInInspector] public NodeState State { get; private set; } = NodeState.Running;
        [HideInInspector] public bool Started { get; private set; } = false;
        [HideInInspector] public float StartTime { get; private set; } = float.MinValue;
        [HideInInspector] public float LastUpdateTime { get; private set; } = float.MinValue;

        public NodeState UpdateNode(BehaviourTreeRunner runner)
        {
            if (!_Initialized)
            {
                Init(runner);
            }

            if (!Started)
            {
                OnStart(runner);
                Started = true;
                StartTime = Time.time;
            }

            State = OnUpdate(runner);

            if (State != NodeState.Running)
            {
                OnStop(runner);
                Started = false;
            }

            LastUpdateTime = Time.time;
            return State;
        }

        internal virtual Node Clone(bool withChildren = true) => Instantiate(this);

        /// <summary>
        /// Called before the first Behaviour Tree update.
        /// </summary>
        /// <param name="runner"></param>
        protected virtual void OnInitialize(BehaviourTreeRunner runner) {}

        /// <summary>
        /// Called when this node starts running
        /// </summary>
        /// <param name="runner"> Runner that started the update. </param>
        protected virtual void OnStart(BehaviourTreeRunner runner) {}

        /// <summary>
        /// Called each time this Node is updated.
        /// </summary>
        /// <param name="runner"> Runner that started the update. </param>
        /// <returns></returns>
        protected abstract NodeState OnUpdate(BehaviourTreeRunner runner);

        /// <summary>
        /// Called when this Node stops running.
        /// </summary>
        /// <param name="runner"> Runner that started the update. </param>
        protected virtual void OnStop(BehaviourTreeRunner runner) {}

        private void Init(BehaviourTreeRunner runner)
        {
            OnInitialize(runner);
            _Initialized = true;
        }
    }

    public enum NodeState
    {
        Running,
        Failure,
        Success
    }
}