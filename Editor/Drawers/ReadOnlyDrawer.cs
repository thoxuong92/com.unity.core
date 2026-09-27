using UnityEditor;
using UnityEngine;
using Unity.Core;

namespace Unity.Core.Editor.Drawers
{
    /// <summary>
    /// Custom Property Drawer cho thuộc tính [ReadOnly], vô hiệu hóa chỉnh sửa trong Inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
