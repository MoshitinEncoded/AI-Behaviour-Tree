using MoshitinEncoded.AI.BehaviourTreeLib;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    [CustomEditor(typeof(Node))]
    public class NodeEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset _VisualTreeAsset;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            _VisualTreeAsset.CloneTree(root);

            var titleField = root.Q<TextField>("title-field");
            titleField.bindingPath = "_Title";

            var node = target as Node;
            var behaviourInspector = new InspectorElement(node.Behaviour);
            behaviourInspector.ClearClassList();
            behaviourInspector.AddToClassList("behaviour-inspector");
            root.Add(behaviourInspector);

            return root;
        }
    }
}
