using UnityEngine;
using MoshitinEncoded.BehaviourTree;

namespace MoshitinEncoded
{
    public class BehaviourTreeMachine : MonoBehaviour
    {
        public enum UpdateModeEnum
        {
            None,
            Update,
            FixedUpdate
        }

        /// <summary>
        /// Called <b>before</b> the update of the behaviour tree. Useful to update parameters.
        /// </summary>
        public event System.Action BehaviourTreeUpdate;

        [SerializeField, Tooltip("The behaviour that will run on this machine.")]
        private BehaviourTreeController _BehaviourTree;

        [SerializeField, Tooltip("How the behaviour will be updated each frame.")]
        private UpdateModeEnum _UpdateMode = UpdateModeEnum.Update;

        /// <summary>
        /// The behaviour tree instance running on this machine.
        /// </summary>
        public BehaviourTreeController BehaviourTree => _BehaviourTree;

        /// <summary>
        /// How the behaviour tree will be updated each frame.
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

            UpdateBehaviour();
        }

        void Update()
        {
            if (_UpdateMode != UpdateModeEnum.Update)
            {
                return;
            }

            UpdateBehaviour();
        }

        /// <summary>
        /// Updates the behaviour tree.
        /// </summary>
        public void UpdateBehaviour()
        {
            if (_BehaviourTree == null)
            {
                return;
            }

            OnBehaviourTreeUpdate();
            BehaviourTreeUpdate?.Invoke();
            _BehaviourTree.Update();
        }

        /// <summary>
        /// Returns the value of a parameter in the behaviour tree.
        /// </summary>
        /// <typeparam name="T"> The parameter Type. Inheritance is supported. </typeparam>
        /// <param name="name"> The parameter name. </param>
        /// <returns> The parameter value if found, <b>default</b> value otherwise. </returns>
        public T GetParameter<T>(string name) =>
            _BehaviourTree.GetParameter<T>(name);

        /// <summary>
        /// Sets the value of a parameter in the behaviour tree.
        /// </summary>
        /// <typeparam name="T"> The parameter Type. Inheritance is supported. </typeparam>
        /// <param name="name"> The parameter name. </param>
        /// <param name="value"> The parameter value to set. </param>
        public void SetParameter<T>(string name, T value) => 
            _BehaviourTree.SetParameter(name, value);

        public void ChangeBehaviourTree(BehaviourTreeController behaviourTree)
        {
            _BehaviourTree = behaviourTree;
            InitializeBehaviourTree();
        }

        /// <summary>
        /// Called <b>after</b> the behaviour tree has been initialized.
        /// </summary>
        protected virtual void OnBehaviourTreeInitialized() {}

        /// <summary>
        /// Called <b>before</b> the update of the behaviour tree. Useful to update parameters.
        /// </summary>
        protected virtual void OnBehaviourTreeUpdate() {}

        private void InitializeBehaviourTree()
        {
            _BehaviourTree = _BehaviourTree.Clone();
            _BehaviourTree.Bind(this);
            OnBehaviourTreeInitialized();
        }
    }
}