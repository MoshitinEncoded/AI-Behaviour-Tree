using UnityEngine;

namespace MoshitinEncoded.AIBehaviourTree
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
        protected BehaviourTreeMachine BehaviourMachine { get; private set; }

        public NodeState Update()
        {
            if (!Started)
            {
                OnStart();
                Started = true;
                StartTime = Time.time;
            }

            State = OnUpdate();

            if (State == NodeState.Failure || State == NodeState.Success)
            {
                OnStop();
                Started = false;
            }

            LastUpdateTime = Time.time;
            return State;
        }

        public virtual Node Clone(bool withChildren = true) => Instantiate(this);

        internal void Bind(BehaviourTreeMachine behaviourMachine) => BehaviourMachine = behaviourMachine;

        protected abstract void OnStart();

        protected abstract NodeState OnUpdate();

        protected abstract void OnStop();
    }

    public enum NodeState
        {
            Running,
            Failure,
            Success
        }
}