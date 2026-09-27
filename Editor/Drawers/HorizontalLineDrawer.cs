using System;
using UnityEditor;
using UnityEngine;
using Unity.Core.Attributes;

namespace Unity.Core.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(HorizontalLineAttribute))]
    public class HorizontalLineDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var hl = (HorizontalLineAttribute)attribute;
            float lineY = position.y + hl.Spacing;

            if (!string.IsNullOrEmpty(hl.Message))
            {
                var content = new GUIContent(hl.Message);
                Vector2 size = EditorStyles.boldLabel.CalcSize(content);
                float msgX = position.x + (position.width - size.x) / 2f;

                // Left line
                float leftWidth = msgX - position.x - 6f - hl.GapSize;
                if (leftWidth > 0)
                {
                    EditorGUI.DrawRect(new Rect(position.x + hl.GapSize, lineY + size.y / 2f - hl.Thickness / 2f, leftWidth, hl.Thickness), hl.Color.ToColor());
                }

                // Label
                EditorGUI.LabelField(new Rect(msgX, lineY, size.x, size.y), content, EditorStyles.boldLabel);

                // Right line
                float rightX = msgX + size.x + 6f;
                float rightWidth = position.x + position.width - hl.GapSize - rightX;
                if (rightWidth > 0)
                {
                    EditorGUI.DrawRect(new Rect(rightX, lineY + size.y / 2f - hl.Thickness / 2f, rightWidth, hl.Thickness), hl.Color.ToColor());
                }
            }
            else
            {
                Rect lineRect = new Rect(position.x + hl.GapSize, lineY, position.width - 2f * hl.GapSize, hl.Thickness);
                EditorGUI.DrawRect(lineRect, hl.Color.ToColor());
            }

            float offset = GetHeaderHeight(hl);
            Rect propRect = new Rect(position.x, position.y + offset, position.width, position.height - offset);
            EditorGUI.PropertyField(propRect, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var hl = (HorizontalLineAttribute)attribute;
            return EditorGUI.GetPropertyHeight(property, label, true) + GetHeaderHeight(hl);
        }

        private float GetHeaderHeight(HorizontalLineAttribute hl)
        {
            float msgHeight = !string.IsNullOrEmpty(hl.Message) ? EditorGUIUtility.singleLineHeight : hl.Thickness;
            return msgHeight + 2f * hl.Spacing;
        }
    }
}
