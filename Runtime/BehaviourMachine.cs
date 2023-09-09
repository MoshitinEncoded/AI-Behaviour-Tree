using UnityEngine;

namespace MoshitinEncoded.AI
{
    public class BehaviourMachine : MonoBehaviour
    {
        public enum UpdateCycleType
        {
            None,
            Update,
            FixedUpdate
        }

        [SerializeField] private BehaviourTree _behaviourTree;
        [SerializeField] private UpdateCycleType _updateCycle;

        public BehaviourTree BehaviourTree => _behaviourTree;

        private void Awake()
        {
            InitializeBehaviourTree();
        }

        private void FixedUpdate()
        {
            if (_updateCycle != UpdateCycleType.FixedUpdate)
            {
                return;
            }

            UpdateTree();
        }

        // Update is called once per frame
        void Update()
        {
            if (_updateCycle != UpdateCycleType.Update)
            {
                return;
            }

            UpdateTree();
        }

        /// <summary>
        /// Updates the behaviour tree once.
        /// </summary>
        public void UpdateTree()
        {
            if (_behaviourTree == null)
            {
                return;
            }

            _behaviourTree.Update();
        }

        public T GetProperty<T>(string name) =>
            _behaviourTree.GetProperty<T>(name);

        public void SetProperty<T>(string name, T value) => 
            _behaviourTree.SetProperty(name, value);

        /// <summary>
        /// Changes the behaviour of this AI. Keep in mind that the behaviour passed here will be cloned as well.
        /// </summary>
        /// <param name="behaviourTree"></param>
        public void ChangeBehaviourTree(BehaviourTree behaviourTree)
        {
            _behaviourTree = behaviourTree;
            InitializeBehaviourTree();
        }

        private void InitializeBehaviourTree()
        {
            _behaviourTree = _behaviourTree.Clone();
            _behaviourTree.Bind(this);
        }
    }
}