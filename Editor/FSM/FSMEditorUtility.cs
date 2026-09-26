using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Unity.Core.FSM.Visual;
using Object = UnityEngine.Object;

namespace Unity.Core.Editor.FSM
{
    public struct CustomConditionInfo
    {
        public string DisplayName;
        public string MethodName;
    }

    /// <summary>
    /// Utility methods for FSM Editor, including locating AIBehaviour directories, scanning scoped Pure C# States,
    /// and discovering [FSMCondition] methods on actors.
    /// </summary>
    public static class FSMEditorUtility
    {
        /// <summary>
        /// Returns all methods on the runner marked with [FSMCondition] returning bool with 0 parameters.
        /// </summary>
        public static List<CustomConditionInfo> GetAvailableCustomConditions(AIBehaviour runner)
        {
            var result = new List<CustomConditionInfo>();
            if (runner == null) return result;

            var methods = runner.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                if (method.ReturnType != typeof(bool) || method.GetParameters().Length != 0) continue;

                var attr = method.GetCustomAttribute<FSMConditionAttribute>();
                if (attr != null)
                {
                    string display = !string.IsNullOrEmpty(attr.DisplayName) ? attr.DisplayName : method.Name;
                    result.Add(new CustomConditionInfo
                    {
                        DisplayName = display,
                        MethodName = method.Name
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Mở file script C# của AIBehaviour và nhảy thẳng tới dòng định nghĩa hàm điều kiện tùy chỉnh methodName.
        /// </summary>
        public static bool OpenCustomConditionScript(AIBehaviour runner, string methodName)
        {
            if (runner == null || string.IsNullOrEmpty(methodName)) return false;

            MonoScript script = MonoScript.FromMonoBehaviour(runner);
            if (script == null) return false;

            string assetPath = AssetDatabase.GetAssetPath(script);
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) return false;

            int lineNumber = 1;
            string[] lines = File.ReadAllLines(assetPath);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Contains(methodName) && (line.Contains("bool ") || line.Contains("bool\t") || line.Contains("FSMCondition")))
                {
                    lineNumber = i + 1;
                    break;
                }
            }

            return AssetDatabase.OpenAsset(script, lineNumber);
        }

        /// <summary>
        /// Gets the project folder path where the AIBehaviour's script resides.
        /// </summary>
        public static string GetAIBehaviourScriptDirectory(AIBehaviour runner)
        {
            if (runner == null) return "Assets";

            // 1. Check if the runner has a custom script component
            MonoScript monoScript = MonoScript.FromMonoBehaviour(runner);
            if (monoScript != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(monoScript);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    string dir = Path.GetDirectoryName(assetPath);
                    if (!string.IsNullOrEmpty(dir))
                    {
                        return dir.Replace("\\", "/");
                    }
                }
            }

            // 2. Fallback to active project directory
            return GetActiveProjectDirectory();
        }

        /// <summary>
        /// Backwards-compatible alias for GetAIBehaviourScriptDirectory.
        /// </summary>
        public static string GetControllerScriptDirectory(AIBehaviour runner) => GetAIBehaviourScriptDirectory(runner);

        /// <summary>
        /// Returns all VisualStateNode types whose script is located in the given directory or any subdirectory.
        /// </summary>
        public static List<Type> GetStatesInDirectory(string directory)
        {
            var result = new List<Type>();
            if (string.IsNullOrEmpty(directory)) return result;

            string normalizedDir = directory.Replace("\\", "/");
            string[] guids = AssetDatabase.FindAssets("t:MonoScript", new[] { normalizedDir });

            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    continue;

                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                if (script == null) continue;

                Type scriptClass = script.GetClass();
                if (scriptClass != null && typeof(VisualStateNode).IsAssignableFrom(scriptClass) && !scriptClass.IsAbstract)
                {
                    if (!result.Contains(scriptClass))
                    {
                        result.Add(scriptClass);
                    }
                }
            }

            // Sort alphabetically for clean UI
            result.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        /// <summary>
        /// Gets the folder path currently selected in Unity's Project window.
        /// </summary>
        public static string GetActiveProjectDirectory()
        {
            string path = "Assets";
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path))
                {
                    if (File.Exists(path))
                    {
                        path = Path.GetDirectoryName(path);
                    }
                    break;
                }
            }

            return string.IsNullOrEmpty(path) ? "Assets" : path.Replace("\\", "/");
        }
    }
}
