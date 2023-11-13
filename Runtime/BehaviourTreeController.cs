using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MoshitinEncoded.BehaviourTree
{
    [CreateAssetMenu(fileName = "NewBehaviourTree", menuName = "MoshitinEncoded/Behaviour Tree")]
    public class BehaviourTreeController : ScriptableObject
    {

#if UNITY_EDITOR
        [SerializeField, HideInInspector] private Vector2 _GraphPosition = Vector2.zero;
        [SerializeField, HideInInspector] private Vector2 _GraphScale = Vector2.one;
#endif

        /// <summary>
        /// Called when this Behaviour Tree has been updated.
        /// </summary>
        public event System.Action Updated;

        [SerializeField] private Node _RootNode;
        [SerializeField] private NodeState _State = NodeState.Running;
        [SerializeField] private List<Node> _Nodes = new();
        [SerializeField] private BlackboardParameter[] _Parameters;

        private BehaviourTreeMachine _BehaviourTreeMachine;

        public Node RootNode => _RootNode;

        public NodeState State => _State;

        public Node[] Nodes => _Nodes.ToArray();

        public BlackboardParameter[] Parameters => _Parameters;

        /// <summary>
        /// Binds this Behaviour Tree with a Behaviour Tree Machine.
        /// </summary>
        /// <param name="behaviourTreeMachine"> The <b>Behaviour Tree Machine</b> to bind with. </param>
        public void Bind(BehaviourTreeMachine behaviourTreeMachine)
        {
            _BehaviourTreeMachine = behaviourTreeMachine;
            Traverse(RootNode, node =>
            {
                node.Bind(behaviourTreeMachine);
            });
        }

        /// <summary>
        /// Updates the Behaviour Tree.
        /// </summary>
        /// <returns> The new state of the Behaviour Tree. </returns>
        public NodeState Update()
        {
            if (RootNode.State == NodeState.Running)
            {
                _State = RootNode.Update();
            }

            Updated?.Invoke();

            return _State;
        }

        /// <summary>
        /// Clones the Behaviour Tree with all his components.
        /// </summary>
        /// <returns> The Behaviour Tree clone. </returns>
        public BehaviourTreeController Clone()
        {
            // Clone behaviour tree and nodes
            BehaviourTreeController clonedTree = Instantiate(this);
            clonedTree._RootNode = RootNode.Clone();

            // Clone nodes list
            clonedTree._Nodes = new List<Node>();
            Traverse(clonedTree.RootNode, node =>
            {
                clonedTree._Nodes.Add(node);
            });

            // Clone properties
            var clonedProperties = new BlackboardParameter[_Parameters.Length];
            for (var i = 0; i < _Parameters.Length; i++)
            {
                clonedProperties[i] = _Parameters[i].Clone();
            }
            clonedTree._Parameters = clonedProperties;

            return clonedTree;
        }

        /// <summary>
        /// Returns the value of a parameter in the behaviour tree.
        /// </summary>
        /// <typeparam name="T"> The parameter Type. Inheritance is supported. </typeparam>
        /// <param name="name"> The parameter name. </param>
        /// <returns> The parameter value if found, <b>default</b> value otherwise. </returns>
        public T GetParameter<T>(string name)
        {
            var property = _Parameters.First(p => p.PropertyName == name);
            if (!property)
            {
                throw new UnityException($"The Behaviour Tree {this.name} does not have a property named \"{name}\" " +
                    $"in \"{_BehaviourTreeMachine.gameObject.name}\".");
            }

            if (property.GetValue() is T value)
            {
                return value;
            }
            else
            {
                throw new UnityException($"The Behaviour Tree {this.name} does not have a {typeof(T).Name} property named \"{name}\" " +
                    $"in \"{_BehaviourTreeMachine.gameObject.name}\".");
            }
        }

        /// <summary>
        /// Sets the value of a parameter in the Behaviour Tree.
        /// </summary>
        /// <typeparam name="T"> The parameter Type. Inheritance is supported. </typeparam>
        /// <param name="name"> The parameter name. </param>
        /// <param name="value"> The parameter value to set. </param>
        public void SetParameter<T>(string name, T value)
        {
            var property = _Parameters.First(p => p.PropertyName == name);
            if (!property)
            {
                throw new UnityException($"The Behaviour Tree {this.name} does not have a property named \"{name}\" " +
                    $"in \"{_BehaviourTreeMachine.gameObject.name}\".");
            }

            var setted = property.SetValue(value);
            if (!setted)
            {
                throw new UnityException($"The Behaviour Tree {this.name} does not have a {typeof(T).Name} property named \"{name}\" " +
                    $"in \"{_BehaviourTreeMachine.gameObject.name}\".");
            }
        }

        private void Traverse(Node node, System.Action<Node> visiter)
        {
            if (node)
            {
                visiter.Invoke(node);
                if (node is IParentNode parentNode)
                {
                    var children = parentNode.GetChildren();
                    children.ForEach(child => Traverse(child, visiter));
                }
            }
        }
    }
}