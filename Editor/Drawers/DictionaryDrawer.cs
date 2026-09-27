using UnityEditor;
using UnityEngine;
using Unity.Core.Collections;

namespace Unity.Core.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(DictionaryAttribute))]
    public class DictionaryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var dictAttr = attribute as DictionaryAttribute;
            string keyLabel = dictAttr?.KeyLabel ?? "Key";
            string valueLabel = dictAttr?.ValueLabel ?? "Value";
            float keyRatio = dictAttr?.KeySize ?? 0.4f;

            SerializedProperty keysProp = property.FindPropertyRelative("keys");
            SerializedProperty valuesProp = property.FindPropertyRelative("values");

            if (keysProp == null || valuesProp == null)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);

            if (!property.isExpanded) return;

            EditorGUI.indentLevel++;
            int count = Mathf.Min(keysProp.arraySize, valuesProp.arraySize);

            // Header
            position.y += EditorGUIUtility.singleLineHeight + 2f;
            float availableWidth = position.width - 30f; // Leave space for delete button
            float keyWidth = availableWidth * keyRatio;
            float valWidth = availableWidth - keyWidth;

            Rect keyHeaderRect = new Rect(position.x, position.y, keyWidth, EditorGUIUtility.singleLineHeight);
            Rect valHeaderRect = new Rect(position.x + keyWidth, position.y, valWidth, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(keyHeaderRect, keyLabel, EditorStyles.miniBoldLabel);
            EditorGUI.LabelField(valHeaderRect, valueLabel, EditorStyles.miniBoldLabel);

            // Rows
            for (int i = 0; i < count; i++)
            {
                position.y += EditorGUIUtility.singleLineHeight + 2f;

                SerializedProperty keyItem = keysProp.GetArrayElementAtIndex(i);
                SerializedProperty valItem = valuesProp.GetArrayElementAtIndex(i);

                Rect kRect = new Rect(position.x, position.y, keyWidth - 2f, EditorGUIUtility.singleLineHeight);
                Rect vRect = new Rect(position.x + keyWidth + 2f, position.y, valWidth - 4f, EditorGUIUtility.singleLineHeight);
                Rect delRect = new Rect(position.x + availableWidth + 4f, position.y, 24f, EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(kRect, keyItem, GUIContent.none);
                EditorGUI.PropertyField(vRect, valItem, GUIContent.none);

                if (GUI.Button(delRect, "X"))
                {
                    keysProp.DeleteArrayElementAtIndex(i);
                    valuesProp.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            // Add Button
            position.y += EditorGUIUtility.singleLineHeight + 4f;
            Rect addRect = new Rect(position.x + position.width - 80f, position.y, 80f, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(addRect, "+ Add"))
            {
                keysProp.arraySize++;
                valuesProp.arraySize++;
            }

            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            SerializedProperty keysProp = property.FindPropertyRelative("keys");
            int count = keysProp != null ? keysProp.arraySize : 0;

            // Foldout + Header + (rows) + Add button + padding
            return EditorGUIUtility.singleLineHeight * (count + 3) + (count + 3) * 2f + 4f;
        }
    }
}
