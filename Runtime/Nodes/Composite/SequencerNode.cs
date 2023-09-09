using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoshitinEncoded.AI
{
    [NodeMenu("Composite/Sequencer")]
    public class SequencerNode : CompositeNode
    {
        private int current;
        protected override void OnStart()
        {
            current = 0;
        }

        protected override void OnStop()
        {
            
        }

        protected override State OnUpdate()
        {
            var child = children[current];
            switch (child.Update())
            {
                case State.Running:
                    return State.Running;
                case State.Failure:
                    return State.Failure;
                case State.Success:
                    current++;
                    break;
            }

            return current == children.Count ? State.Success : State.Running;
        }
    }
}