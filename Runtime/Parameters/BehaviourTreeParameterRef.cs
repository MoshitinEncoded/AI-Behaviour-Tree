using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [System.Serializable]
    public class BehaviourTreeParameterRef<T>
    {
        [SerializeField] private string _Name;
        
        private BehaviourTreeParameter<T> _Parameter;

        public T Value => _Parameter.Value;

        /// <summary>
        /// Binds this parameter to the Behaviour Tree of a runner.
        /// </summary>
        /// <param name="runner"></param>
        public void Bind(BehaviourTreeRunner runner)
        {
            _Parameter = runner.GetParameterByRef<T>(_Name);
        }
    }
}
