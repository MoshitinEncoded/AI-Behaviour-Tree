using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoshitinEncoded.AI
{
    [NodeMenu("Action/Wait")]
    public class WaitNode : ActionNode
    {
        public float duration = 1f;
        float startTime;

        protected override void OnStart()
        {
            startTime = Time.time;
        }

        protected override State OnUpdate()
        {
            if (TimeOver())
            {
                return State.Success;
            }

            return State.Running;
        }

        protected override void OnStop()
        {
            
        }

        private bool TimeOver() =>
            Time.time - startTime >= duration;
    }
}