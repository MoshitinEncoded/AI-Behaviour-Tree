using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MoshitinEncoded.AI
{
    [CreateAssetMenu(fileName = "NewBehaviourTree", menuName = "MoshitinEncoded/AI/Behaviour Tree")]
    public class BehaviourTree : ScriptableObject
    {
        public Node rootNode;
        [SerializeField] private Node.State m_State = Node.State.Running;
        [SerializeField] private List<Node> m_Nodes = new();
        [SerializeField] private BlackboardProperty[] _properties;
        private BehaviourMachine m_behaviourMachine;

#if UNITY_EDITOR
        [SerializeField, HideInInspector] private Vector2 _graphPosition = Vector2.zero;
        [SerializeField, HideInInspector] private Vector2 _graphScale = Vector2.one;
#endif

        public Node.State State => m_State;
        public List<Node> Nodes => m_Nodes;
        public BlackboardProperty[] Properties => _properties;

        internal void Bind(BehaviourMachine behaviourMachine)
        {
            m_behaviourMachine = behaviourMachine;
            Traverse(rootNode, node =>
            {
                node.Bind(behaviourMachine);
            });
        }

        public Node.State Update()
        {
            if (rootNode.state == Node.State.Running)
            {
                m_State = rootNode.Update();
            }

            return m_State;
        }

        public void Traverse(Node node, System.Action<Node> visiter)
        {
            if (node)
            {
                visiter.Invoke(node);
                var children = GetChildren(node);
                children.ForEach(child => Traverse(child, visiter));
            }
        }

        public List<Node> GetChildren(Node parent)
        {
            if (parent is RootNode rootNode && rootNode.child != null)
            {
                return new List<Node>() { rootNode.child };
            }
            else if (parent is DecoratorNode decorator && decorator.child != null)
            {
                return new List<Node>() { decorator.child };
            }
            else if (parent is CompositeNode composite)
            {
                return composite.children;
            }

            return new List<Node>();
        }

        public BehaviourTree Clone()
        {
            // Clone behaviour tree and nodes
            BehaviourTree clonedTree = Instantiate(this);
            clonedTree.rootNode = rootNode.Clone();

            // Clone nodes list
            clonedTree.m_Nodes = new List<Node>();
            Traverse(clonedTree.rootNode, node =>
            {
                clonedTree.m_Nodes.Add(node);
            });

            // Clone properties
            var clonedProperties = new BlackboardProperty[_properties.Length];
            for (var i = 0; i < _properties.Length; i++)
            {
                clonedProperties[i] = _properties[i].Clone();
            }
            clonedTree._properties = clonedProperties;

            return clonedTree;
        }

        internal T GetProperty<T>(string name)
        {
            var property = _properties.First(p => p.PropertyName == name);
            if (property != null && property.Value is T value)
            {
                return value;
            }
            else
            {
                throw new UnityException($"GetProperty exception in \"{m_behaviourMachine.gameObject.name}\" GameObject." +
                    $"{this.name} behaviour tree does not have a {typeof(T).Name} property \"{name}\".");
            }
        }

        internal void SetProperty<T>(string name, T value)
        {
            var property = _properties.First(p => p.PropertyName == name);
            if (property != null && property.Value is T)
            {
                property.Value = value;
            }
            else
            {
                throw new UnityException($"SetProperty exception in \"{m_behaviourMachine.gameObject.name}\" GameObject." +
                    $"{this.name} behaviour tree does not have a {typeof(T).Name} property \"{name}\".");
            }
        }
    }
}