#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
#endif
using UnityEngine;

namespace MoshitinEncoded.AI
{
    public class BlackboardProperty : ScriptableObject
    {
        public virtual object Value {get; set;}
        public string PropertyName
        {
            get
            {
                return _propertyName;
            }
            set
            {
                _propertyName = value;
            }
        }

        [SerializeField] private string _propertyName;

#if UNITY_EDITOR
        public bool IsExpanded;
        public virtual void DrawProperty(BlackboardSection propertiesSection) { }

        internal BlackboardProperty Clone() =>
            Instantiate(this);
#endif
    }

    public class BlackboardProperty<T> : BlackboardProperty
    {
        [SerializeField] private T _value;
        public override object Value
        {
            get => _value;
            set
            {
                if (value is T newValue)
                {
                    _value = newValue;
                }
            }
        }
#if UNITY_EDITOR
        public override void DrawProperty(BlackboardSection propertiesSection)
        {
            // Get the property type text
            string typeText;
            var propertyAttribute = GetType().GetCustomAttribute<AddPropertyMenuAttribute>();

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
            var valueProperty = thisSerialized.FindProperty("_value");

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