using MoshitinEncoded.AI.BehaviourTreeLib;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(RepeaterNode))]
public class RepeaterNodeEditor : Editor
{
    [SerializeField] private VisualTreeAsset _VisualTreeAsset;

    private VisualElement _Root;

    public override VisualElement CreateInspectorGUI()
    {
        _Root = new VisualElement();
        _VisualTreeAsset.CloneTree(_Root);
        _Root.Q("script").SetEnabled(false);

        var stopModeProperty = serializedObject.FindProperty("_StopMode");
        UpdateStopModeVisibility(stopModeProperty.enumValueIndex);

        var useParameterProperty = serializedObject.FindProperty("_UseParameter");
        UpdateUseParameterVisibility(useParameterProperty.boolValue);

        var stopModeElement = _Root.Q<PropertyField>("stop-mode");
        stopModeElement.RegisterValueChangeCallback(OnStopModeChanged);

        var useParameterElement = _Root.Q<PropertyField>("use-parameter");
        useParameterElement.RegisterValueChangeCallback(OnUseParameterChanged);

        return _Root;
    }

    private void OnUseParameterChanged(SerializedPropertyChangeEvent evt) =>
        UpdateUseParameterVisibility(evt.changedProperty.boolValue);

    private void OnStopModeChanged(SerializedPropertyChangeEvent evt) =>
        UpdateStopModeVisibility(evt.changedProperty.enumValueIndex);

    private void UpdateStopModeVisibility(int enumValueIndex)
    {
        var repeatContainer = _Root.Q("repeat-container");
        var timerElement = _Root.Q("timer");

        if (enumValueIndex == (int)RepeaterNode.StopMode.Time)
        {
            repeatContainer.style.display = DisplayStyle.None;
            timerElement.style.display = DisplayStyle.Flex;
        }
        else if (enumValueIndex == (int)RepeaterNode.StopMode.None)
        {
            repeatContainer.style.display = DisplayStyle.None;
            timerElement.style.display = DisplayStyle.None;
        }
        else
        {
            repeatContainer.style.display = DisplayStyle.Flex;
            timerElement.style.display = DisplayStyle.None;
        }
    }

    private void UpdateUseParameterVisibility(bool useParameter)
    {
        var repeatTimesElement = _Root.Q("repeat-times");
        var repeatTimesParameterElement = _Root.Q("repeat-times-parameter");

        if (useParameter)
        {
            repeatTimesElement.style.display = DisplayStyle.None;
            repeatTimesParameterElement.style.display = DisplayStyle.Flex;
        }
        else
        {
            repeatTimesElement.style.display = DisplayStyle.Flex;
            repeatTimesParameterElement.style.display = DisplayStyle.None;
        }
    }
}
