using System.Collections.Generic;
using UnityEngine;

namespace MoshitinEncoded.BehaviourTree
{
    public class RootNode : Node, IParentNode
    {
        [SerializeField, HideInInspector] private Node _Child;

        public Node Child => _Child;

        protected override void OnStart()
        {
            
        }

        protected override void OnStop()
        {
            
        }

        protected override NodeState OnUpdate() =>
            _Child.Update();

        public override Node Clone(bool withChild)
        {
            RootNode rootNode = Instantiate(this);
            if (_Child != null)
            {
                rootNode._Child = _Child.Clone();
            }
            
            return rootNode;
        }

        public void AddChild(Node child)
        {
            if (child != null)
            {
                _Child = child;
            }
        }

        public List<Node> GetChildren()
        {
            if (_Child != null)
            {
                return new List<Node>() { _Child };
            }

            return new List<Node>();
        }

        public void ClearChildren()
        {
            _Child = null;
        }
    }
}