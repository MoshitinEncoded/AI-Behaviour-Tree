using UnityEngine;
using MoshitinEncoded.AI.BehaviourTreeLib;
using MoshitinEncoded.GraphTools;

namespace MoshitinEncoded.AI
{
    public class BehaviourTreeRunner : MonoBehaviour
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
        public event System.Action WillUpdate;

        /// <summary>
        /// Called after the Behaviour Tree update.
        /// </summary>
        public event System.Action Updated;

        [SerializeField]
        private BehaviourTree _BehaviourTree;

        [SerializeField, Tooltip("How and when the Behaviour Tree will be updated.")]
        private UpdateModeEnum _UpdateMode = UpdateModeEnum.Update;

        private BehaviourTree _BehaviourTreeInstance;

        /// <summary>
        /// The Behaviour Tree asset.
        /// </summary>
        public BehaviourTree BehaviourTree => _BehaviourTree;

        /// <summary>
        /// The Behaviour Tree instance running on this machine.
        /// </summary>
        public BehaviourTree BehaviourTreeInstance => _BehaviourTreeInstance;

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

            WillUpdate?.Invoke();

            _BehaviourTreeInstance.UpdateBehaviour(runner: this);

            Updated?.Invoke();
        }

        /// <summary>
        /// Changes the active Behaviour Tree. This action causes a new initialization.
        /// </summary>
        /// <param name="behaviourTree"></param>
        public void ChangeBehaviourTree(BehaviourTree behaviourTree)
        {
            _BehaviourTree = behaviourTree;
            InitializeBehaviourTree();
        }

        /// <summary>
        /// Returns the value of a parameter in the Behaviour Tree.
        /// </summary>
        /// <typeparam name="T"> Parameter Type. </typeparam>
        /// <param name="name"> Parameter name as shown on the blackboard. </param>
        /// <returns> The parameter value if found, <b>default</b> otherwise. </returns>
        public T GetParameter<T>(string name)
        {
            var parameter = GetParameterByRef<T>(name);
            return parameter != null ? parameter.Value : default;
        }

        /// <summary>
        /// Sets the value of a parameter in the Behaviour Tree.
        /// </summary>
        /// <typeparam name="T"> Parameter Type. </typeparam>
        /// <param name="name"> Parameter name as shown on the blackboard. </param>
        /// <param name="value"> New parameter value. </param>
        public void SetParameter<T>(string name, T value)
        {
            var parameter = GetParameterByRef<T>(name);
            if (parameter)
            {
                parameter.Value = value;
            }
        }

        /// <summary>
        /// Returns a parameter in the Behaviour Tree.
        /// </summary>
        /// <param name="name"> Parameter name as shown on the Blackboard. </param>
        /// <returns> The parameter if found, <b>null</b> otherwise. </returns>
        public BlackboardParameter GetParameterByRef(string name)
        {
            var parameter = _BehaviourTreeInstance.GetParameter(name);

            if (parameter == null)
            {
                Debug.LogError(
                    message: $"Parameter \"{name}\" doesn't exist in {_BehaviourTree.name} Behaviour Tree.",
                    context: gameObject);
            }

            return parameter;
        }

        /// <summary>
        /// Returns a parameter in the Behaviour Tree.
        /// </summary>
        /// <typeparam name="T"> Parameter Type. </typeparam>
        /// <param name="name"> Parameter name as shown on the Blackboard. </param>
        /// <returns> The parameter if found, <b>null</b> otherwise. </returns>
        public BlackboardParameter<T> GetParameterByRef<T>(string name)
        {
            var parameter = _BehaviourTreeInstance.GetParameter<T>(name);

            if (parameter == null)
            {
                Debug.LogError(
                    message: $"Parameter \"{name}: {typeof(T).Name}\" doesn't exist in {_BehaviourTree.name} Behaviour Tree.",
                    context: gameObject);
            }

            return parameter;
        }

        private void InitializeBehaviourTree()
        {
            _BehaviourTreeInstance = _BehaviourTree.Clone();
            Initialized?.Invoke();
        }
    }
}