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

        [HideInInspector] public NodeState State { get; private set; } = NodeState.Running;
        [HideInInspector] public bool Started { get; private set; } = false;
        [HideInInspector] public float StartTime { get; private set; } = float.MinValue;
        [HideInInspector] public float LastUpdateTime { get; private set; } = float.MinValue;

        public NodeState UpdateNode(BehaviourTreeRunner runner)
        {
            if (!Started)
            {
                OnStart(runner);
                Started = true;
                StartTime = Time.time;
            }

            State = OnUpdate(runner);

            if (State == NodeState.Failure || State == NodeState.Success)
            {
                OnStop(runner);
                Started = false;
            }

            LastUpdateTime = Time.time;
            return State;
        }

        public virtual Node Clone(bool withChildren = true) => Instantiate(this);

        /// <summary>
        /// Called when this node starts running
        /// </summary>
        /// <param name="runner"> Runner that started the update. </param>
        protected abstract void OnStart(BehaviourTreeRunner runner);

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
        protected abstract void OnStop(BehaviourTreeRunner runner);
    }

    public enum NodeState
        {
            Running,
            Failure,
            Success
        }
}