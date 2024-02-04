using MoshitinEncoded.AI.BehaviourTreeLib;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(BehaviourTreeParameterRef<>))]
public class BehaviourTreeParameterRefDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement root = new();
        var nameField = new PropertyField(property.FindPropertyRelative("_Name"), property.displayName);
        root.Add(nameField);

        return root;
    }
}
