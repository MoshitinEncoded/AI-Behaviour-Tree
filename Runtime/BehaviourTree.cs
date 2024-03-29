using System;
using System.Collections.Generic;

using MoshitinEncoded.GraphTools;

using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    [CreateAssetMenu(fileName = "NewBehaviourTree", menuName = "MoshitinEncoded/Behaviour Tree")]
    public class BehaviourTree : ScriptableObject
    {

#if UNITY_EDITOR
        [SerializeField, HideInInspector] private Vector2 _GraphPosition = new(512,200);
        [SerializeField, HideInInspector] private Vector2 _GraphScale = Vector2.one;
#endif

        /// <summary>
        /// Called when this Behaviour Tree has been updated.
        /// </summary>
        public event Action Updated;

        [SerializeField, HideInInspector] private Node _RootNode;
        [SerializeField, HideInInspector] private NodeState _State = NodeState.Running;
        [SerializeField, HideInInspector] private List<Node> _Nodes = new();
        [SerializeField, HideInInspector] private Blackboard _Blackboard;

        public Node RootNode => _RootNode;

        public NodeState State => _State;

        public Node[] Nodes => _Nodes.ToArray();

        public Blackboard Blackboard => _Blackboard;

        /// <summary>
        /// Updates the Behaviour Tree.
        /// </summary>
        /// <returns> The new state of the Behaviour Tree. </returns>
        public NodeState UpdateBehaviour(BehaviourTreeRunner runner)
        {
            if (!RootNode) return NodeState.Failure;

            if (RootNode.State == NodeState.Running)
            {
                _State = RootNode.RunBehaviour(runner);
            }

            Updated?.Invoke();

            return _State;
        }

        /// <summary>
        /// Clones the Behaviour Tree with all his components.
        /// </summary>
        /// <returns> The Behaviour Tree clone. </returns>
        public BehaviourTree Clone(BlackboardParameterOverride[] parameterOverrides = null)
        {
            // Clone behaviour tree and nodes
            BehaviourTree clonedTree = Instantiate(this);
            
            clonedTree._RootNode = RootNode.Clone();

            // Clone nodes list
            clonedTree._Nodes = new List<Node>();
            Traverse(clonedTree.RootNode, node =>
            {
                clonedTree._Nodes.Add(node);
            });

            // Clone blackboard
            clonedTree._Blackboard = Blackboard.CloneAndOverride(parameterOverrides);

            return clonedTree;
        }

        /// <summary>
        /// Returns a parameter in the Behaviour Tree.
        /// </summary>
        /// <param name="name"> Parameter name as shown on the Blackboard. </param>
        internal BlackboardParameter GetParameter(string name)
        {
            if (!Blackboard)
            {
                throw new UnityException(
                    $"An unexpected error ocurred with {name} Behaviour Tree. Try to exit play mode if you didn't and" +
                    "select it in the project window to refresh it. If this doesn't solve the error, create a new one.");
            }

            return Blackboard.GetParameter(name);
        }

        /// <summary>
        /// Returns a parameter in the Behaviour Tree.
        /// </summary>
        /// <param name="name"> Parameter name as shown on the Blackboard. </param>
        internal BlackboardParameter<T> GetParameter<T>(string name)
        {
            if (!Blackboard)
            {
                throw new UnityException(
                    $"An unexpected error ocurred with {name} Behaviour Tree. Try to exit play mode if you didn't and" +
                    "select it in the project window to refresh it. If this doesn't solve the error, create a new one.");
            }

            return Blackboard.GetParameter<T>(name);
        }

        private void Traverse(Node node, Action<Node> visiter)
        {
            if (node)
            {
                visiter.Invoke(node);
                foreach (var child in node.Children)
                {
                    Traverse(child, visiter);
                }
            }
        }
    }
}