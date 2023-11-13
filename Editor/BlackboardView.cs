using MoshitinEncoded.BehaviourTree;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MoshitinEncoded.Editor.BehaviourTree
{
    internal class BlackboardView : Blackboard
    {
        private BehaviourTreeController _tree;
        private SerializedObject _serializedTree;
        private BlackboardSection _propertiesSection;

        public BlackboardView(GraphView graphView) : base(graphView)
        {
            subTitle = "Blackboard";
            SetPosition(new Rect(10, 10, 300, 300));

            addItemRequested += OnAddProperty;
            editTextRequested += OnEditFieldText;
            moveItemRequested += OnMoveProperty;
        }

        public void PopulateView(BehaviourTreeController tree, SerializedObject serializedTree)
        {
            if (_tree == tree)
            {
                SavePropertiesExpandedState();
            }
            else
            {
                _tree = tree;
                _serializedTree = serializedTree;
                title = _tree.name;

                ResetPropertiesExpandedState();
            }

            Clear();

            _propertiesSection = new BlackboardSection() { title = "Exposed Properties" };
            DrawProperties();

            Add(_propertiesSection);
        }

        private void ResetPropertiesExpandedState()
        {
            foreach (var property in _tree.Parameters)
            {
                property.IsExpanded = false;
            }
        }

        internal void DeleteProperty(BlackboardField blackboardField)
        {
            var propertyToDelete = _tree.Parameters.First(p => p.PropertyName == blackboardField.text);

            if (propertyToDelete != null)
            {
                _serializedTree.Update();
                _serializedTree.FindProperty("_Parameters").RemoveFromObjectArray(propertyToDelete);
                _serializedTree.ApplyModifiedProperties();

                Undo.DestroyObjectImmediate(propertyToDelete);
            }

            var blackboardRow = blackboardField.GetFirstAncestorOfType<BlackboardRow>();
            _propertiesSection.Remove(blackboardRow);
        }

        private void DrawProperties()
        {
            foreach (var property in _tree.Parameters)
            {
                property.DrawProperty(_propertiesSection);
            }
        }

        private void SavePropertiesExpandedState()
        {
            foreach (var property in _tree.Parameters)
            {
                foreach (var blackboardRow in this.Query<BlackboardRow>().ToList())
                {
                    if (property.PropertyName == blackboardRow.Q<BlackboardField>().text)
                    {
                        property.IsExpanded = blackboardRow.expanded;
                    }
                }
            }
        }

        private void OnAddProperty(Blackboard blackboard)
        {
            var menu = new GenericMenu();

            var propertyTypes = TypeCache.GetTypesDerivedFrom(typeof(BlackboardParameter<>));
            foreach (var propertyType in propertyTypes)
            {
                var propertyAttribute = propertyType.GetCustomAttribute<AddParameterMenuAttribute>();
                if (propertyAttribute != null)
                {
                    menu.AddItem(new GUIContent(propertyAttribute.MenuPath), false, AddPropertyOfType, propertyType);
                }
            }

            menu.ShowAsContext();
        }

        private void AddPropertyOfType(object propertyType)
        {
            // Create the property
            var newProperty = ScriptableObject.CreateInstance((Type)propertyType) as BlackboardParameter;
            var propertyName = "NewProperty";

            // Create a unique property name
            var index = 0;
            while (_tree.Parameters.Any(p => p.name == propertyName))
            {
                index++;
                propertyName = $"NewProperty ({index})";
            }

            newProperty.name = propertyName;
            newProperty.PropertyName = propertyName;

            // Add the property to the asset
            AssetDatabase.AddObjectToAsset(newProperty, _tree);
            Undo.RegisterCreatedObjectUndo(newProperty, "Create Property (Behaviour Tree)");

            // Draw the property on the properties section
            newProperty.DrawProperty(_propertiesSection);

            // Add the property to the behaviour tree
            _serializedTree.Update();
            _serializedTree.FindProperty("_Parameters").AddToObjectArray(newProperty);
            _serializedTree.ApplyModifiedProperties();
        }

        private void OnEditFieldText(Blackboard blackboard, VisualElement element, string newName)
        {
            if (newName == "")
            {
                return;
            }

            var fieldToChange = element as BlackboardField;
            if (fieldToChange != null)
            {
                var property = _tree.Parameters.First(p => p.PropertyName == fieldToChange.text);

                var index = 0;
                var fieldText = newName;
                var blackboardFields = blackboard.Query<BlackboardField>().ToList();
                while (blackboardFields.Any(field => field != fieldToChange && field.text == fieldText))
                {
                    index++;
                    fieldText = $"{newName} ({index})";
                }

                fieldToChange.text = fieldText;

                Undo.RecordObject(property, "Change Property Name (Behaviour Tree)");
                property.name = fieldText;
                property.PropertyName = fieldText;
            }
        }

        private void OnMoveProperty(Blackboard blackboard, int newIndex, VisualElement element)
        {
            var blackboardField = element as BlackboardField;
            if (blackboardField != null)
            {
                var srcIndex = -1;
                for (var i = 0; i < _tree.Parameters.Length; i++)
                {
                    if (_tree.Parameters[i].name == blackboardField.text)
                    {
                        srcIndex = i;
                        break;
                    }
                }

                if (srcIndex == -1)
                {
                    return;
                }

                if (srcIndex < newIndex)
                {
                    newIndex--;
                }
                
                var blackboardRow = _propertiesSection.ElementAt(srcIndex);
                _propertiesSection.Remove(blackboardRow);
                _propertiesSection.Insert(newIndex, blackboardRow);

                _serializedTree.Update();
                _serializedTree.FindProperty("_Parameters").MoveArrayElement(srcIndex, newIndex);
                _serializedTree.ApplyModifiedProperties();
            }
        }
    }
}