using System.Reflection;
using UnityEditor;
using UnityEngine;
using Unity.Core.Attributes;

namespace Unity.Core.Editor.Drawers
{
    /// <summary>
    /// Vẽ gizmos trực quan (Circle, Cube, Line, Sphere) trực tiếp trong Scene View
    /// cho các field được gắn thuộc tính DrawCircle, DrawCube, DrawLine, DrawSphere.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneGizmoDrawer
    {
        static SceneGizmoDrawer()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (Selection.activeGameObject == null) return;

            var components = Selection.activeGameObject.GetComponents<MonoBehaviour>();
            foreach (var comp in components)
            {
                if (comp == null) continue;
                DrawComponentGizmos(comp);
            }
        }

        private static void DrawComponentGizmos(MonoBehaviour comp)
        {
            var fields = comp.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                // Circle
                var circleAttr = field.GetCustomAttribute<DrawCircleAttribute>();
                if (circleAttr != null)
                {
                    Vector3 pos = GetVector3FromField(comp, field, comp.transform.position);
                    Handles.color = circleAttr.Color.ToColor();
                    Quaternion rot = circleAttr.Rotation switch
                    {
                        QuationCircle.XZ => Quaternion.Euler(90, 0, 0),
                        QuationCircle.YZ => Quaternion.Euler(0, 90, 0),
                        _ => Quaternion.identity
                    };
                    Handles.CircleHandleCap(0, pos, rot, circleAttr.Radius, EventType.Repaint);
                }

                // Cube
                var cubeAttr = field.GetCustomAttribute<DrawCubeAttribute>();
                if (cubeAttr != null)
                {
                    Vector3 pos = GetVector3FromField(comp, field, comp.transform.position);
                    Handles.color = cubeAttr.Color.ToColor();
                    Handles.CubeHandleCap(0, pos, comp.transform.rotation, cubeAttr.Size, EventType.Repaint);
                }

                // Sphere
                var sphereAttr = field.GetCustomAttribute<DrawSphereAttribute>();
                if (sphereAttr != null)
                {
                    Vector3 pos = GetVector3FromField(comp, field, comp.transform.position);
                    Handles.color = sphereAttr.Color.ToColor();
                    Handles.SphereHandleCap(0, pos, Quaternion.identity, sphereAttr.Radius * 2f, EventType.Repaint);
                }

                // Line
                var lineAttr = field.GetCustomAttribute<DrawLineAttribute>();
                if (lineAttr != null)
                {
                    Vector3 targetPos = GetVector3FromField(comp, field, comp.transform.position + comp.transform.forward);
                    Handles.color = lineAttr.Color.ToColor();
                    Handles.DrawLine(comp.transform.position, targetPos);
                }
            }
        }

        private static Vector3 GetVector3FromField(MonoBehaviour comp, FieldInfo field, Vector3 defaultPos)
        {
            object val = field.GetValue(comp);
            if (val is Vector3 v3) return comp.transform.TransformPoint(v3);
            if (val is Vector2 v2) return comp.transform.TransformPoint(new Vector3(v2.x, v2.y, 0));
            if (val is Transform t && t != null) return t.position;
            if (val is GameObject g && g != null) return g.transform.position;
            return defaultPos;
        }
    }
}
