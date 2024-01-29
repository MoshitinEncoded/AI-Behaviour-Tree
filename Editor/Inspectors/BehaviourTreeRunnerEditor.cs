using MoshitinEncoded.AI;
using MoshitinEncoded.AI.BehaviourTreeLib;
using MoshitinEncoded.GraphTools;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    [CustomEditor(typeof(BehaviourTreeRunner))]
    public class BehaviourTreeRunnerEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset _VisualTreeAsset;
        private VisualElement _Root;

        private void OnEnable()
        {
            Undo.undoRedoPerformed += DrawParameters;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= DrawParameters;
            UnregisterBehaviourTreeChangeCallback();
        }

        public override VisualElement CreateInspectorGUI()
        {
            _Root = new()
            {
                viewDataKey = target.GetInstanceID().ToString()
            };
            
            _Root.Clear();
            _VisualTreeAsset.CloneTree(_Root);

            RegisterBehaviourTreeChangeCallback();
            DrawParameters();
            
            return _Root;
        }

        private void RegisterBehaviourTreeChangeCallback()
        {
            var behaviourTreeElement = _Root.Q("behaviour-tree");
            behaviourTreeElement?.RegisterCallback<ChangeEvent<Object>>(OnBehaviourTreeChange);
        }

        private void UnregisterBehaviourTreeChangeCallback()
        {
            var behaviourTreeElement = _Root.Q("behaviour-tree");
            behaviourTreeElement?.UnregisterCallback<ChangeEvent<Object>>(OnBehaviourTreeChange);
        }

        private void OnBehaviourTreeChange(ChangeEvent<Object> evt)
        {
            if (Application.isPlaying)
            {
                return;
            }

            var parametersFoldout = GetFoldout();
            if (evt.previousValue != null && parametersFoldout != null)
            {
                parametersFoldout.Clear();
            }

            if (evt.newValue != null)
            {
                DrawParameters();
            }
        }

        private void DrawParameters()
        {
            var runner = target as BehaviourTreeRunner;

            var parametersFoldout = GetFoldout();
            if (parametersFoldout != null)
            {
                parametersFoldout.Clear();
            }
            else
            {
                parametersFoldout = CreateFoldout();
                _Root.Add(parametersFoldout);
            }

            var behaviourTree = Application.isPlaying ? runner.BehaviourTreeInstance : runner.BehaviourTree;
            if (behaviourTree == null)
            {
                return;
            }

            var parameters = behaviourTree.Blackboard.Parameters;

            if (parameters.Length == 0)
            {
                return;
            }

            var parametersContainer = new Box();
            TrackParameters(parametersContainer, behaviourTree);

            if (Application.isPlaying)
            {
                DrawInstanceParameters(parameters, parametersContainer);
            }
            else
            {
                DrawParameter(runner, parameters, parametersContainer);
            }

            parametersFoldout.Add(parametersContainer);
        }

        private Foldout GetFoldout() =>_Root.Q<Foldout>("parameters-foldout");

        private Foldout CreateFoldout()
        {
            var foldout = new Foldout()
            {
                name = "parameters-foldout",
                text = "Parameters",
                viewDataKey = "parameters-foldout"
            };

            return foldout;
        }

        private void TrackParameters(VisualElement element, BehaviourTree behaviourTree)
        {
            element.TrackPropertyValue(new SerializedObject(behaviourTree.Blackboard).FindProperty("_Parameters"), _ =>
            {
                DrawParameters();
            });
        }

        private void DrawInstanceParameters(BlackboardParameter[] parameters, VisualElement container)
        {
            foreach (var parameter in parameters)
            {
                var serializedParameter = new SerializedObject(parameter);
                var parameterField = CreateParameterField(
                    serializedValueParameter: serializedParameter,
                    labelParameter: parameter,
                    serializedLabelParameter: serializedParameter
                );

                if (serializedParameter.FindProperty("_Value").propertyType == SerializedPropertyType.Generic)
                {
                    parameterField.AddToClassList("instance-multi-line-parameter");
                }

                container.Add(parameterField);
            }
        }

        private void DrawParameter(BehaviourTreeRunner runner, BlackboardParameter[] parameters, VisualElement container)
        {
            foreach (var parameter in parameters)
            {
                var parameterOverride = GetParameterOverride(parameter, runner.ParameterOverrides);
                var overrideToggle = CreateOverrideToggle(parameterOverride);

                var valueParameter = parameterOverride ? parameterOverride : parameter;
                var serializedValueParameter = new SerializedObject(valueParameter);

                var parameterField = CreateParameterField(
                    serializedValueParameter: serializedValueParameter,
                    labelParameter: parameter,
                    serializedLabelParameter: new SerializedObject(parameter));

                if (!parameterOverride)
                {
                    parameterField.SetEnabled(false);
                }

                parameterField.Bind(serializedValueParameter);

                overrideToggle.RegisterCallback<ChangeEvent<bool>>(
                    evt => OnOverrideToggleChange(evt, parameterField, parameter, serializedObject));

                var parameterElement = new VisualElement();
                if (serializedValueParameter.FindProperty("_Value").propertyType == SerializedPropertyType.Generic)
                {
                    parameterElement.AddToClassList("multi-line-parameter");
                }
                else
                {
                    parameterElement.AddToClassList("single-line-parameter");
                }

                parameterElement.Add(overrideToggle);
                parameterElement.Add(parameterField);

                container.Add(parameterElement);
            }
        }

        private static Toggle CreateOverrideToggle(bool overridden)
        {
            return new Toggle()
            {
                name = "override-toggle",
                text = "",
                value = overridden,
                tooltip = "Override."
            };
        }

        private PropertyField CreateParameterField(SerializedObject serializedValueParameter, BlackboardParameter labelParameter, SerializedObject serializedLabelParameter)
        {
            var valueProperty = serializedValueParameter.FindProperty("_Value");
            var labelProperty = serializedLabelParameter.FindProperty("_ParameterName");
            var parameterField = new PropertyField(valueProperty, labelProperty.stringValue);

            parameterField.Bind(serializedValueParameter);

            parameterField.TrackPropertyValue(
                property: serializedLabelParameter.FindProperty("_ParameterName"),
                callback: _ => RenameParameter(_, labelParameter, parameterField));

            return parameterField;
        }

        private void RenameParameter(SerializedProperty serializedParameter, BlackboardParameter parameter, PropertyField parameterField)
        {
            var runner = target as BehaviourTreeRunner;
            parameterField.label = serializedParameter.stringValue;
            parameterField.Q<Label>().text = serializedParameter.stringValue;
            if (Application.isPlaying)
            {
                return;
            }

            var parameterOverride = GetParameterOverride(parameter, runner.ParameterOverrides);
            if (parameterOverride != null)
            {
                var serializedParameterOverride = new SerializedObject(parameterOverride);
                serializedParameterOverride.FindProperty("_ParameterName").stringValue = serializedParameter.stringValue;
                serializedParameterOverride.ApplyModifiedProperties();
            }
        }

        private BlackboardParameter GetParameterOverride(BlackboardParameter parameter, BlackboardParameterOverride[] parameterOverrides)
        {
            for (var i = 0; i < parameterOverrides.Length; i++)
            {
                if (parameterOverrides[i].OriginalParameter == parameter)
                {
                    return parameterOverrides[i].OverrideParameter;
                }
            }

            return null;
        }

        private void OnOverrideToggleChange(
            ChangeEvent<bool> evt, PropertyField parameterField, BlackboardParameter parameter, SerializedObject serializedRunner)
        {
            if (evt.newValue == true)
            {
                var parameterOverride = CreateInstance<BlackboardParameterOverride>();
                Undo.RegisterCreatedObjectUndo(parameterOverride, "Override Parameter (Behaviour Tree)");

                var serializedParameterOverride = new SerializedObject(parameterOverride);
                serializedParameterOverride.FindProperty("_OriginalParameter").objectReferenceValue = parameter;

                var overrideParameter = Instantiate(parameter);
                Undo.RegisterCreatedObjectUndo(overrideParameter, "Override Parameter (Behaviour Tree)");

                serializedParameterOverride.FindProperty("_OverrideParameter").objectReferenceValue = overrideParameter;
                serializedParameterOverride.ApplyModifiedProperties();
                
                serializedRunner.Update();
                serializedRunner.FindProperty("_ParameterOverrides").AddToArray(parameterOverride);
                serializedRunner.ApplyModifiedProperties();

                parameterField.Bind(new SerializedObject(overrideParameter));
                parameterField.SetEnabled(true);
            }
            else
            {
                serializedRunner.Update();
                var parameterOverridesProperty = serializedRunner.FindProperty("_ParameterOverrides");
                for (var i = 0; i < parameterOverridesProperty.arraySize; i++)
                {
                    var arrayElement = parameterOverridesProperty.GetArrayElementAtIndex(i);
                    var parameterOverride = new SerializedObject(arrayElement.objectReferenceValue);
                    var originalParameter = parameterOverride.FindProperty("_OriginalParameter").objectReferenceValue;
                    if (originalParameter == parameter)
                    {
                        parameterOverridesProperty.RemoveFromArray(arrayElement.objectReferenceValue);
                        serializedRunner.ApplyModifiedProperties();
                        break;
                    }
                }

                parameterField.Bind(new SerializedObject(parameter));
                parameterField.SetEnabled(false);
            }
        }
    }
}
