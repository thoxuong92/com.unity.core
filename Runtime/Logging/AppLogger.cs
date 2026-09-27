using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Unity.Core.Logging
{
    /// <summary>
    /// Conditional Logger for Unity Core.
    /// In Release builds (without ENABLE_UNITY_CORE_LOG define symbol), all logging calls
    /// are stripped out completely by the C# compiler, eliminating GC allocs & Logcat exposure.
    /// </summary>
    public static class AppLogger
    {
        [Conditional("ENABLE_UNITY_CORE_LOG")]
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Log(string message, Object context = null)
        {
            Debug.Log($"<color=#4CAF50>[UnityCore]</color> {message}", context);
        }

        [Conditional("ENABLE_UNITY_CORE_LOG")]
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(string message, Object context = null)
        {
            Debug.LogWarning($"<color=#FF9800>[UnityCore Warning]</color> {message}", context);
        }

        public static void LogError(string message, Object context = null)
        {
            Debug.LogError($"<color=#F44336>[UnityCore Error]</color> {message}", context);
        }

        public static void LogException(System.Exception exception, Object context = null)
        {
            Debug.LogException(exception, context);
        }
    }
}
