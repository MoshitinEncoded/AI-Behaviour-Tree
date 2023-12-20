using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Task/Debug/Log Message")]
    public class LogMessageNode : TaskNode
    {
        [Space]
        [Tooltip("Message to print on the console.")]
        [SerializeField] private string _Message;
        [SerializeField] private LogType _LogType;

        protected override NodeState Run(BehaviourTreeRunner runner)
        {
            switch (_LogType)
            {
                case LogType.Log:
                    Debug.Log(_Message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(_Message);
                    break;
                case LogType.Error:
                    Debug.LogError(_Message);
                    break;
            }

            return NodeState.Success;
        }

        private enum LogType
        {
            Log,
            Warning,
            Error
        }
    }
}