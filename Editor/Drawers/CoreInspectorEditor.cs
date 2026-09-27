using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Unity.Core.Attributes;

namespace Unity.Core.Editor.Drawers
{
    /// <summary>
    /// Fallback Custom Editor cho mọi MonoBehaviour trong dự án:
    /// - Tự động tạo Button tương tác cho các hàm đánh dấu bằng [Button] (kèm nhập tham số int, float, string, bool).
    /// - Hiển thị và cho phép chỉnh sửa biến tĩnh đánh dấu bằng [Static] trực tiếp trong Play Mode.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
    public class CoreInspectorEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, object[]> _methodParamValues = new Dictionary<string, object[]>();
        private bool _staticFoldOut = false;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DrawStaticFields();
            DrawMethodButtons();
        }

        private void DrawStaticFields()
        {
            if (target == null) return;

            var type = target.GetType();
            var staticFields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            List<FieldInfo> markedFields = new List<FieldInfo>();
            foreach (var f in staticFields)
            {
                if (f.IsDefined(typeof(StaticAttribute), false))
                {
                    markedFields.Add(f);
                }
            }

            if (markedFields.Count == 0) return;

            EditorGUILayout.Space(8f);
            _staticFoldOut = EditorGUILayout.Foldout(_staticFoldOut, $"Static Variables ({markedFields.Count})", true, EditorStyles.foldoutHeader);
            if (!_staticFoldOut) return;

            EditorGUI.indentLevel++;
            foreach (var field in markedFields)
            {
                object val = field.GetValue(null);
                string label = $"{field.Name} ({field.FieldType.Name})";

                if (field.FieldType == typeof(int))
                {
                    int newVal = EditorGUILayout.IntField(label, (int)(val ?? 0));
                    if (newVal != (int)(val ?? 0)) field.SetValue(null, newVal);
                }
                else if (field.FieldType == typeof(float))
                {
                    float newVal = EditorGUILayout.FloatField(label, (float)(val ?? 0f));
                    if (!Mathf.Approximately(newVal, (float)(val ?? 0f))) field.SetValue(null, newVal);
                }
                else if (field.FieldType == typeof(string))
                {
                    string newVal = EditorGUILayout.TextField(label, (string)(val ?? ""));
                    if (newVal != (string)(val ?? "")) field.SetValue(null, newVal);
                }
                else if (field.FieldType == typeof(bool))
                {
                    bool newVal = EditorGUILayout.Toggle(label, (bool)(val ?? false));
                    if (newVal != (bool)(val ?? false)) field.SetValue(null, newVal);
                }
                else
                {
                    EditorGUILayout.LabelField(label, val != null ? val.ToString() : "null");
                }
            }
            EditorGUI.indentLevel--;
        }

        private void DrawMethodButtons()
        {
            if (target == null) return;

            var methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var btnAttr = method.GetCustomAttribute<ButtonAttribute>(true);
                if (btnAttr == null) continue;

                // Check enable mode
                bool enabled = btnAttr.SelectedEnableMode switch
                {
                    EButtonEnableMode.Always => true,
                    EButtonEnableMode.Editor => !EditorApplication.isPlaying,
                    EButtonEnableMode.Playmode => EditorApplication.isPlaying,
                    _ => true
                };

                string btnName = string.IsNullOrEmpty(btnAttr.Text) ? method.Name : btnAttr.Text;
                var parameters = method.GetParameters();

                EditorGUILayout.Space(6f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUI.BeginDisabledGroup(!enabled);
                if (GUILayout.Button(btnName, GUILayout.Height(28f)))
                {
                    ExecuteMethod(method, parameters);
                }
                EditorGUI.EndDisabledGroup();

                if (parameters.Length > 0)
                {
                    DrawParameterInputs(method.Name, parameters);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawParameterInputs(string methodName, ParameterInfo[] parameters)
        {
            if (!_methodParamValues.TryGetValue(methodName, out var paramArray) || paramArray.Length != parameters.Length)
            {
                paramArray = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].HasDefaultValue)
                    {
                        paramArray[i] = parameters[i].DefaultValue;
                    }
                    else if (parameters[i].ParameterType == typeof(int)) paramArray[i] = 0;
                    else if (parameters[i].ParameterType == typeof(float)) paramArray[i] = 0f;
                    else if (parameters[i].ParameterType == typeof(string)) paramArray[i] = "";
                    else if (parameters[i].ParameterType == typeof(bool)) paramArray[i] = false;
                }
                _methodParamValues[methodName] = paramArray;
            }

            EditorGUI.indentLevel++;
            for (int i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                if (p.ParameterType == typeof(int))
                {
                    paramArray[i] = EditorGUILayout.IntField(p.Name, (int)(paramArray[i] ?? 0));
                }
                else if (p.ParameterType == typeof(float))
                {
                    paramArray[i] = EditorGUILayout.FloatField(p.Name, (float)(paramArray[i] ?? 0f));
                }
                else if (p.ParameterType == typeof(string))
                {
                    paramArray[i] = EditorGUILayout.TextField(p.Name, (string)(paramArray[i] ?? ""));
                }
                else if (p.ParameterType == typeof(bool))
                {
                    paramArray[i] = EditorGUILayout.Toggle(p.Name, (bool)(paramArray[i] ?? false));
                }
                else
                {
                    EditorGUILayout.LabelField(p.Name, $"Type {p.ParameterType.Name} not editable inline");
                }
            }
            EditorGUI.indentLevel--;
        }

        private void ExecuteMethod(MethodInfo method, ParameterInfo[] parameters)
        {
            foreach (var t in targets)
            {
                if (t == null) continue;

                try
                {
                    object[] args = null;
                    if (parameters.Length > 0 && _methodParamValues.TryGetValue(method.Name, out var values))
                    {
                        args = values;
                    }

                    method.Invoke(method.IsStatic ? null : t, args);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CoreInspectorEditor] Error executing {method.Name}: {ex.InnerException?.Message ?? ex.Message}");
                }
            }
        }
    }
}
