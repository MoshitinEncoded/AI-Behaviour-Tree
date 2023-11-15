#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
#endif
using UnityEngine;

namespace MoshitinEncoded.AI.BehaviourTreeLib
{
    public abstract class BlackboardParameter : ScriptableObject
    {
        [SerializeField] private string _PropertyName;
        public string PropertyName
        {
            get => _PropertyName;
            set => _PropertyName = value;
        }

        public virtual object GetValue() { return null; }
        public virtual bool SetValue(object value) { return true; }
#if UNITY_EDITOR
        public bool IsExpanded;
        public virtual void DrawProperty(BlackboardSection propertiesSection) { }

        internal BlackboardParameter Clone() =>
            Instantiate(this);
#endif
    }

    public class BlackboardParameter<T> : BlackboardParameter
    {
        [SerializeField] private T _Value;

        public override object GetValue()
        {
            return _Value;
        }

        public override bool SetValue(object value)
        {
            if (value is T newValue)
            {
                _Value = newValue;
                return true;
            }
            else if (value == null)
            {
                _Value = default;
                return true;
            }
            else
            {
                return false;
            }
        }
#if UNITY_EDITOR
        public override void DrawProperty(BlackboardSection propertiesSection)
        {
            // Get the property type text
            string typeText;
            var propertyAttribute = GetType().GetCustomAttribute<AddParameterMenuAttribute>();

            if (propertyAttribute != null)
            {
                var propertyMenuPath = propertyAttribute.MenuPath.Split('/');
                typeText = propertyMenuPath.Last();
            }
            else
            {
                typeText = typeof(T).Name;
            }

            // Create the property field
            var blackboardField = new BlackboardField()
            {
                text = PropertyName,
                typeText = typeText
            };

            // Create the property value field
            var thisSerialized = new SerializedObject(this);
            var valueProperty = thisSerialized.FindProperty("_Value");

            var propertyValueField = new PropertyField(valueProperty);
            propertyValueField.Bind(thisSerialized);

            // Create the blackboard row that contains the property
            var blackboardRow = new BlackboardRow(item: blackboardField, propertyView: propertyValueField)
            {
                expanded = IsExpanded
            };

            // Add the property to the blackboard
            propertiesSection.Add(blackboardRow);
        }
#endif
    }
}