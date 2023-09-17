using UnityEngine;

namespace MoshitinEncoded.AI
{
    public abstract class Node : ScriptableObject
    {
        public enum State
        {
            Running,
            Failure,
            Success
        }

        public string Title = "Node";
        [HideInInspector] public State state = State.Running;
        [HideInInspector] public bool Started { get; private set; } = false;
#if UNITY_EDITOR
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 position;
#endif
        protected BehaviourMachine BehaviourMachine { get; private set; }

        public State Update()
        {
            if (!Started)
            {
                OnStart();
                Started = true;
            }

            state = OnUpdate();

            if (state == State.Failure || state == State.Success)
            {
                OnStop();
                Started = false;
            }

            return state;
        }

        internal void Bind(BehaviourMachine behaviourMachine) =>
            BehaviourMachine = behaviourMachine;

        public virtual Node Clone(bool withChildren = true) => Instantiate(this);
        protected abstract void OnStart();
        protected abstract State OnUpdate();
        protected abstract void OnStop();
    }
}