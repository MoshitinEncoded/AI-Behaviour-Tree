using UnityEngine;
using MoshitinEncoded.BehaviourTree;

namespace MoshitinEncoded
{
    public class BehaviourTreeMachine : MonoBehaviour
    {
        public enum UpdateModeEnum
        {
            /// <summary>
            /// The Behaviour Tree is not updated at all. You must call <i>UpdateBehaviourTree</i> yourself.
            /// </summary>
            None,
            /// <summary>
            /// The Behaviour Tree is updated each frame.
            /// </summary>
            Update,
            /// <summary>
            /// The Behaviour Tree is updated each physics frame.
            /// </summary>
            FixedUpdate
        }

        /// <summary>
        /// Called after <i>Awake</i> and <i>ChangeBehaviourTree</i> methods. Useful to <b>set</b> parameters.
        /// </summary>
        public event System.Action Initialized;

        /// <summary>
        /// Called before the Behaviour Tree update. Useful to <b>update</b> parameters.
        /// </summary>
        public event System.Action PreUpdate;

        [SerializeField, Tooltip("The Behaviour Tree that will run on this machine.")]
        private BehaviourTreeController _BehaviourTree;

        [SerializeField, Tooltip("How the Behaviour Tree will be updated.")]
        private UpdateModeEnum _UpdateMode = UpdateModeEnum.Update;

        private BehaviourTreeController _BehaviourTreeInstance;

        /// <summary>
        /// The Behaviour Tree asset.
        /// </summary>
        public BehaviourTreeController BehaviourTree => _BehaviourTree;

        /// <summary>
        /// The Behaviour Tree instance running on this machine.
        /// </summary>
        public BehaviourTreeController BehaviourTreeInstance => _BehaviourTreeInstance;

        /// <summary>
        /// How the Behaviour Tree will be updated.
        /// </summary>
        public UpdateModeEnum UpdateMode { get => _UpdateMode; set => _UpdateMode = value; }

        private void Awake()
        {
            InitializeBehaviourTree();
        }

        private void FixedUpdate()
        {
            if (_UpdateMode != UpdateModeEnum.FixedUpdate)
            {
                return;
            }

            UpdateBehaviourTree();
        }

        private void Update()
        {
            if (_UpdateMode != UpdateModeEnum.Update)
            {
                return;
            }

            UpdateBehaviourTree();
        }

        public void UpdateBehaviourTree()
        {
            if (_BehaviourTreeInstance == null)
            {
                return;
            }

            OnPreUpdate();
            PreUpdate?.Invoke();
            _BehaviourTreeInstance.Update();
        }

        /// <summary>
        /// Changes the active Behaviour Tree. This action causes a new initialization.
        /// </summary>
        /// <param name="behaviourTree"></param>
        public void ChangeBehaviourTree(BehaviourTreeController behaviourTree)
        {
            _BehaviourTree = behaviourTree;
            InitializeBehaviourTree();
        }

        /// <summary>
        /// Returns the value of a parameter in the Behaviour Tree.
        /// </summary>
        /// <typeparam name="T"> The parameter Type. Inheritance is supported. </typeparam>
        /// <param name="name"> The parameter name. </param>
        /// <returns> The parameter value if found, <b>default</b> value otherwise. </returns>
        public T GetParameter<T>(string name) =>
            _BehaviourTreeInstance.GetParameter<T>(name);

        /// <summary>
        /// Sets the value of a parameter in the Behaviour Tree.
        /// </summary>
        /// <typeparam name="T"> The parameter Type. Inheritance is supported. </typeparam>
        /// <param name="name"> The parameter name. </param>
        /// <param name="value"> The parameter value to set. </param>
        public void SetParameter<T>(string name, T value) => 
            _BehaviourTreeInstance.SetParameter(name, value);

        /// <summary>
        /// Called after <i>Awake</i> and <i>ChangeBehaviourTree</i> methods. Useful to <b>set</b> parameters.
        /// </summary>
        protected virtual void OnInitialized() {}

        /// <summary>
        /// Called before the Behaviour Tree update. Useful to <b>update</b> parameters.
        /// </summary>
        protected virtual void OnPreUpdate() {}

        private void InitializeBehaviourTree()
        {
            _BehaviourTreeInstance = _BehaviourTree.Clone();
            _BehaviourTreeInstance.Bind(this);
            OnInitialized();
            Initialized?.Invoke();
        }
    }
}