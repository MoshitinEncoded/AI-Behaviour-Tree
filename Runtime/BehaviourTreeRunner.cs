using System;

using MoshitinEncoded.AI.BehaviourTreeLib;
using MoshitinEncoded.GraphTools;

using UnityEngine;

namespace MoshitinEncoded.AI
{
    public class BehaviourTreeRunner : MonoBehaviour
    {
        /// <summary>
        /// Called after <i>Awake</i> and <i>ChangeBehaviourTree</i> methods. Useful to <b>set</b> parameters.
        /// </summary>
        public event Action Initialized;

        /// <summary>
        /// Called before the Behaviour Tree update. Useful to <b>update</b> parameters.
        /// </summary>
        public event Action WillUpdate;

        /// <summary>
        /// Called after the Behaviour Tree update.
        /// </summary>
        public event Action Updated;

        [SerializeField] private BehaviourTree _BehaviourTree;

        [Tooltip("How and when the Behaviour Tree will be updated.")]
        [SerializeField] private TreeUpdateMode _UpdateMode = TreeUpdateMode.Update;

        [SerializeField] private BlackboardParameterOverride[] _ParameterOverrides;

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
        public TreeUpdateMode UpdateMode { get => _UpdateMode; set => _UpdateMode = value; }

        public BlackboardParameterOverride[] ParameterOverrides => _ParameterOverrides;

        private void Awake()
        {
            InitializeBehaviourTree();
        }

        private void FixedUpdate()
        {
            if (_UpdateMode != TreeUpdateMode.FixedUpdate)
            {
                return;
            }

            UpdateBehaviourTree();
        }

        private void Update()
        {
            if (_UpdateMode != TreeUpdateMode.Update)
            {
                return;
            }

            UpdateBehaviourTree();
        }

        /// <summary>
        /// Updates the Behaviour Tree.
        /// </summary>
        /// <returns> The state of the Behaviour Tree. </returns>
        public NodeState UpdateBehaviourTree()
        {
            if (_BehaviourTreeInstance == null)
            {
                return NodeState.Failure;
            }

            WillUpdate?.Invoke();

            var state = _BehaviourTreeInstance.UpdateBehaviour(runner: this);

            Updated?.Invoke();
            return state;
        }

        /// <summary>
        /// Restarts the Behaviour Tree completely. This action causes a new initialization.
        /// </summary>
        public void RestartBehaviourTree()
        {
            InitializeBehaviourTree();
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
            if (_BehaviourTreeInstance == null)
            {
                LogMissingBehaviourTreeError();
                return null;
            }
            
            var parameter = _BehaviourTreeInstance.GetParameter(name);
            if (parameter == null)
            {
                LogMissingParameterWarning();
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
            if (_BehaviourTreeInstance == null)
            {
                LogMissingBehaviourTreeError();
                return null;
            }
            
            var parameter = _BehaviourTreeInstance.GetParameter<T>(name);
            if (parameter == null)
            {
                LogMissingParameterWarning(typeof(T));
            }

            return parameter;
        }

        private void InitializeBehaviourTree()
        {
            if (_BehaviourTree == null) return;

            _BehaviourTreeInstance = _BehaviourTree.Clone(_ParameterOverrides);
            Initialized?.Invoke();
        }

        private void LogMissingBehaviourTreeError()
        {
            Debug.LogError(
                message: $"BehaviourTree Error: there is no Behaviour Tree to get the parameter from.",
                context: gameObject);
        }

        private void LogMissingParameterWarning(Type parameterType = null)
        {
            var typeText = parameterType != null ? $": {parameterType.Name}" : "";
            Debug.LogWarning(
                message: $"BehaviourTree Warning: parameter \"{name}{typeText}\" doesn't exist in {_BehaviourTree.name} Behaviour Tree.",
                context: gameObject);
        }
    }

    public enum TreeUpdateMode
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
        /// The Behaviour Tree is updated each fixed update frame.
        /// </summary>
        FixedUpdate
    }
}