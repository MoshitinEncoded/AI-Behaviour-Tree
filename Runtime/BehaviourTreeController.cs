using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MoshitinEncoded.BehaviourTree
{
    [CreateAssetMenu(fileName = "NewBehaviourTree", menuName = "MoshitinEncoded/Behaviour Tree")]
    public class BehaviourTreeController : ScriptableObject
    {
        /// <summary>
        /// Called <b>after</b> the update of this behaviour.
        /// </summary>
        public event System.Action Updated;
        [SerializeField] private Node _RootNode;
        [SerializeField] private Node.State _State = Node.State.Running;
        [SerializeField] private List<Node> _Nodes = new();
        [SerializeField] private BlackboardParameter[] _Parameters;

#if UNITY_EDITOR
        [SerializeField, HideInInspector] private Vector2 _GraphPosition = Vector2.zero;
        [SerializeField, HideInInspector] private Vector2 _GraphScale = Vector2.one;
#endif

        private BehaviourTreeMachine _BehaviourTreeMachine;

        public Node RootNode { get => _RootNode; private set => _RootNode = value; }

        /// <summary>
        /// State of the <b>Behaviour Tree</b>.
        /// </summary>
        public Node.State State => _State;

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
        /// <returns> The state of the Behaviour Tree. </returns>
        public Node.State Update()
        {
            if (RootNode.state == Node.State.Running)
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
            clonedTree.RootNode = RootNode.Clone();

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
            if (property != null && property.Value is T value)
            {
                return value;
            }
            else
            {
                throw new UnityException($"GetProperty exception in \"{_BehaviourTreeMachine.gameObject.name}\" GameObject." +
                    $"{this.name} behaviour tree does not have a {typeof(T).Name} property \"{name}\".");
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
            if (property != null && property.IsOfType(typeof(T)))
            {
                property.Value = value;
            }
            else
            {
                throw new UnityException($"SetProperty exception in \"{_BehaviourTreeMachine.gameObject.name}\" GameObject." +
                    $"{this.name} behaviour tree does not have a {typeof(T).Name} property \"{name}\".");
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