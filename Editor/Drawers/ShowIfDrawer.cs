using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Unity.Core.Attributes;

namespace Unity.Core.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ShouldShow(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ShouldShow(property) ? EditorGUI.GetPropertyHeight(property, label, true) : 0f;
        }

        private bool ShouldShow(SerializedProperty property)
        {
            var showIf = (ShowIfAttribute)attribute;
            SerializedProperty conditionProp = FindConditionProperty(property, showIf.ConditionField);

            bool conditionMet = false;
            if (conditionProp != null)
            {
                conditionMet = EvaluateCondition(conditionProp, showIf);
            }
            else
            {
                // Fallback to reflection on target object
                conditionMet = EvaluateByReflection(property, showIf);
            }

            return showIf.Invert ? !conditionMet : conditionMet;
        }

        private SerializedProperty FindConditionProperty(SerializedProperty property, string name)
        {
            string propertyPath = property.propertyPath;
            int lastDot = propertyPath.LastIndexOf('.');
            if (lastDot >= 0)
            {
                string containerPath = propertyPath.Substring(0, lastDot);
                string relativePath = containerPath + "." + name;
                var found = property.serializedObject.FindProperty(relativePath);
                if (found != null) return found;
            }

            return property.serializedObject.FindProperty(name);
        }

        private bool EvaluateCondition(SerializedProperty prop, ShowIfAttribute attr)
        {
            return prop.propertyType switch
            {
                SerializedPropertyType.Boolean => prop.boolValue.Equals(attr.ExpectedValue ?? true),
                SerializedPropertyType.Enum => prop.enumValueIndex.ToString().Equals(attr.ExpectedValue?.ToString()) ||
                                               prop.enumNames[prop.enumValueIndex].Equals(attr.ExpectedValue?.ToString()),
                SerializedPropertyType.Integer => prop.intValue.ToString().Equals(attr.ExpectedValue?.ToString()),
                SerializedPropertyType.Float => Mathf.Approximately(prop.floatValue, Convert.ToSingle(attr.ExpectedValue ?? 0f)),
                SerializedPropertyType.String => string.Equals(prop.stringValue, attr.ExpectedValue?.ToString(), StringComparison.Ordinal),
                SerializedPropertyType.ObjectReference => (prop.objectReferenceValue != null).Equals(attr.ExpectedValue ?? true),
                _ => true
            };
        }

        private bool EvaluateByReflection(SerializedProperty property, ShowIfAttribute attr)
        {
            object target = property.serializedObject.targetObject;
            if (target == null) return true;

            var type = target.GetType();
            var field = type.GetField(attr.ConditionField, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                object val = field.GetValue(target);
                return val != null && val.Equals(attr.ExpectedValue ?? true);
            }

            var prop = type.GetProperty(attr.ConditionField, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null)
            {
                object val = prop.GetValue(target);
                return val != null && val.Equals(attr.ExpectedValue ?? true);
            }

            return true;
        }
    }
}
