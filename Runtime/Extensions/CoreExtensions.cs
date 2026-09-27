using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.Core.Extensions
{
    public static class CoreExtensions
    {
        /// <summary>
        /// Chuyển đổi tên tài nguyên / khóa theo nền tảng (Android_ / iOS_).
        /// </summary>
        public static string ToPlatformName(this string name)
        {
#if UNITY_ANDROID
            return "Android_" + name;
#elif UNITY_IOS
            return "iOS_" + name;
#else
            return name;
#endif
        }

        public static bool IsNullOrEmpty<T>(this T input) where T : ICollection
        {
            return input == null || input.Count == 0;
        }

        public static bool IsNullOrEmpty<T>(this T[] input)
        {
            return input == null || input.Length == 0;
        }

        public static bool IsInBuildSettings(string sceneName = null)
        {
#if UNITY_EDITOR
            string target = string.IsNullOrEmpty(sceneName) ? SceneManager.GetActiveScene().name : sceneName;
            var scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                string s = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
                if (string.Equals(s, target, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
#else
            return true;
#endif
        }

        public static Vector3 GetScreenPosition(this Camera camera, Vector3 targetPosition)
        {
            if (camera == null) camera = Camera.main;
            return camera != null ? camera.WorldToScreenPoint(targetPosition) : Vector3.zero;
        }

        public static bool IsTargetVisible(this Vector3 screenPosition)
        {
            return screenPosition.z > 0 &&
                   screenPosition.x > 0 && screenPosition.x < Screen.width &&
                   screenPosition.y > 0 && screenPosition.y < Screen.height;
        }

        public static void GetArrowIndicatorPositionAndAngle(ref Vector3 screenPosition, ref float angle, Vector3 screenCentre, Vector3 screenBounds)
        {
            screenPosition -= screenCentre;

            if (screenPosition.z < 0)
            {
                screenPosition *= -1;
            }

            angle = Mathf.Atan2(screenPosition.y, screenPosition.x);
            float slope = Mathf.Tan(angle);

            if (screenPosition.x > 0)
            {
                screenPosition = new Vector3(screenBounds.x, screenBounds.x * slope, 0);
            }
            else
            {
                screenPosition = new Vector3(-screenBounds.x, -screenBounds.x * slope, 0);
            }

            if (screenPosition.y > screenBounds.y)
            {
                screenPosition = new Vector3(screenBounds.y / slope, screenBounds.y, 0);
            }
            else if (screenPosition.y < -screenBounds.y)
            {
                screenPosition = new Vector3(-screenBounds.y / slope, -screenBounds.y, 0);
            }

            screenPosition += screenCentre;
        }

        public static void DestroyChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                    continue;
                }
#endif
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        public static void SetLayerRecursively(this GameObject obj, int newLayer)
        {
            if (obj == null) return;
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                if (child != null)
                {
                    SetLayerRecursively(child.gameObject, newLayer);
                }
            }
        }

        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}
