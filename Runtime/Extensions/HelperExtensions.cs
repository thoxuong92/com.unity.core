using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Unity.Core.Extensions
{
    public static class HelperExtensions
    {
        [Conditional("UNITY_EDITOR")]
        public static void CheckFilled(this MonoBehaviour @object, Type attributeType)
        {
            if (@object == null) return;

            var fields = @object.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (Attribute.IsDefined(field, attributeType) && field.GetValue(@object) == null)
                {
                    UnityEngine.Debug.LogError($"[InspectorCheck] '{field.Name}' on '{@object.transform.GetPathString()}' is null! Please assign it in the inspector for '{@object.GetType().Name}'.", @object);
                }
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void CheckFilled(this object @object, Transform owner, Type attributeType)
        {
            if (@object == null) return;

            var fields = @object.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (Attribute.IsDefined(field, attributeType) && field.GetValue(@object) == null)
                {
                    string path = owner != null ? owner.GetPathString() : "Unknown";
                    UnityEngine.Debug.LogError($"[InspectorCheck] '{field.Name}' on '{path}' is null! Please assign it in the inspector for '{@object.GetType().Name}'.");
                }
            }
        }

        public static string GetPathString(this Transform child)
        {
            if (child == null) return string.Empty;
            return child.gameObject.scene.name + "/" + string.Join("/", GetHierarchy(child).Select(t => t.name));
        }

        private static List<Transform> GetHierarchy(Transform child)
        {
            if (child == null) return new List<Transform>();
            if (child.parent == null) return new List<Transform> { child };

            var path = GetHierarchy(child.parent);
            path.Add(child);
            return path;
        }
    }
}
