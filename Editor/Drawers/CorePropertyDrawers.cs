using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.Core.Attributes;
using Object = UnityEngine.Object;

namespace Unity.Core.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ProgressAttribute))]
    public class ProgressDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var prog = (ProgressAttribute)attribute;
            if (property.propertyType == SerializedPropertyType.Float)
            {
                float val = Mathf.Clamp(property.floatValue, prog.Min, prog.Max);
                float percent = (val - prog.Min) / (prog.Max - prog.Min);

                Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
                Rect barRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);

                EditorGUI.LabelField(labelRect, label);
                EditorGUI.ProgressBar(barRect, percent, $"{val:F1} / {prog.Max:F1} ({percent * 100f:F0}%)");
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }

    [CustomPropertyDrawer(typeof(TagAttribute))]
    public class TagDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }

    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = EditorGUI.LayerField(position, label, property.intValue);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }

    [CustomPropertyDrawer(typeof(SceneAttribute))]
    public class SceneDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                var scenes = EditorBuildSettings.scenes;
                if (scenes.Length == 0)
                {
                    EditorGUI.PropertyField(position, property, label);
                    return;
                }

                var sceneNames = scenes.Select(s => Path.GetFileNameWithoutExtension(s.path)).ToArray();
                int currentIndex = Mathf.Max(0, Array.IndexOf(sceneNames, property.stringValue));

                int newIndex = EditorGUI.Popup(position, label.text, currentIndex, sceneNames);
                if (newIndex >= 0 && newIndex < sceneNames.Length)
                {
                    property.stringValue = sceneNames[newIndex];
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }

    [CustomPropertyDrawer(typeof(ForceFillAttribute))]
    public class ForceFillDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool isNull = property.propertyType switch
            {
                SerializedPropertyType.ObjectReference => property.objectReferenceValue == null,
                SerializedPropertyType.String => string.IsNullOrEmpty(property.stringValue),
                _ => false
            };

            if (isNull)
            {
                Rect warnRect = new Rect(position.x, position.y, position.width, 22f);
                EditorGUI.HelpBox(warnRect, $"[ForceFill] Field '{label.text}' must not be null!", MessageType.Error);

                Rect propRect = new Rect(position.x, position.y + 24f, position.width, position.height - 24f);
                EditorGUI.PropertyField(propRect, property, label, true);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool isNull = property.propertyType switch
            {
                SerializedPropertyType.ObjectReference => property.objectReferenceValue == null,
                SerializedPropertyType.String => string.IsNullOrEmpty(property.stringValue),
                _ => false
            };

            float baseHeight = EditorGUI.GetPropertyHeight(property, label, true);
            return isNull ? baseHeight + 24f : baseHeight;
        }
    }

    [CustomPropertyDrawer(typeof(PreviewAttribute))]
    public class PreviewDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var previewAttr = (PreviewAttribute)attribute;
            float thumbSize = previewAttr.ThumbnailSize;

            if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null)
            {
                float fieldWidth = position.width - thumbSize - 6f;
                Rect propRect = new Rect(position.x, position.y, fieldWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(propRect, property, label);

                Rect thumbRect = new Rect(position.x + fieldWidth + 6f, position.y, thumbSize, thumbSize);
                Texture2D preview = AssetPreview.GetAssetPreview(property.objectReferenceValue);
                if (preview != null)
                {
                    GUI.DrawTexture(thumbRect, preview, ScaleMode.ScaleToFit);
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var previewAttr = (PreviewAttribute)attribute;
            if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null)
            {
                return Mathf.Max(EditorGUIUtility.singleLineHeight, previewAttr.ThumbnailSize);
            }
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }

    [CustomPropertyDrawer(typeof(URLAttribute))]
    public class URLDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var url = (URLAttribute)attribute;
            float btnWidth = 60f;

            Rect propRect = new Rect(position.x, position.y, position.width - btnWidth - 4f, position.height);
            EditorGUI.PropertyField(propRect, property, label, true);

            Rect btnRect = new Rect(position.x + position.width - btnWidth, position.y, btnWidth, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(btnRect, url.Label ?? "Open"))
            {
                Application.OpenURL(url.Link);
            }
        }
    }
}
