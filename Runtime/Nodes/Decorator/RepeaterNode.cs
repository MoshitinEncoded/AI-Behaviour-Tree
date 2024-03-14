using System.ComponentModel;

using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateNodeMenu("Decorator/Repeater")]
    [Tooltip("Keeps running its child for any amount of times (or forever).")]
    public class RepeaterNode : DecoratorNode
    {
        public enum StopMode
        {
            /// <summary>
            /// Will never stop.
            /// </summary>
            None,
            /// <summary>
            /// Will stop after X amount of ticks. <br />
            /// Each time the node runs, it is considered as one tick.
            /// </summary>
            Ticks,
            /// <summary>
            /// Will stop after X amount of cycles. <br />
            /// One cycle is completed when the child returns success or failure.
            /// </summary>
            Cycles,
            /// <summary>
            /// Will stop after X seconds.
            /// </summary>
            Time
        }

        [Tooltip("Determines how the repeater will stop running.")]
        [SerializeField] private StopMode _StopMode = StopMode.None;

        [Tooltip("Whether you want to use a parameter or a constant value.")]
        [SerializeField] private bool _UseParameter;

        [Tooltip("Amount of ticks or cycles to go through before stopping.")]
        [SerializeField] private int _RepeatTimes;

        [Tooltip("Parameter to use as repeat times.")]
        [SerializeField] private string _RepeatTimesParameter;

        [SerializeField] private Timer _Timer;

        private int _RepeatCount = 0;

        protected override void OnStart(BehaviourTreeRunner runner)
        {
            if (_StopMode == StopMode.Time)
            {
                _Timer.Play();
            }
        }

        protected override NodeState Run(BehaviourTreeRunner runner)
        {
            if (!Child)
            {
                return NodeState.Failure;
            }

            var childState = Child.RunBehaviour(runner);

            return _StopMode switch
            {
                StopMode.None => NodeState.Running,
                StopMode.Ticks => RepeatByTicks(runner),
                StopMode.Cycles => RepeatByCycles(runner, childState),
                StopMode.Time => RepeatByTime(childState),
                _ => LogInvalidStopModeException(runner),
            };
        }

        private NodeState RepeatByTicks(BehaviourTreeRunner runner)
        {
            _RepeatCount++;
            return _RepeatCount >= GetRunTimes(runner)
                ? NodeState.Success
                : NodeState.Running;
        }

        private NodeState RepeatByCycles(BehaviourTreeRunner runner, NodeState childState)
        {
            if (childState == NodeState.Running)
            {
                return NodeState.Running;
            }

            _RepeatCount++;
            if (_RepeatCount >= GetRunTimes(runner))
            {
                return NodeState.Success;
            }
            return NodeState.Running;
        }

        private NodeState RepeatByTime(NodeState childState) =>
            _Timer.IsOver && childState != NodeState.Running ? NodeState.Success : NodeState.Running;
        
        private NodeState LogInvalidStopModeException(BehaviourTreeRunner runner)
        {
            Debug.LogException(
                new InvalidEnumArgumentException(nameof(_StopMode), (int)_StopMode, typeof(StopMode)),
                this
            );

            return NodeState.Failure;
        }

        private int GetRunTimes(BehaviourTreeRunner runner) =>
            _UseParameter ? runner.GetParameterValue<int>(_RepeatTimesParameter) : _RepeatTimes;
    }
}