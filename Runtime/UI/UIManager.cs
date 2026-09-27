using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Core.Logging;

namespace Unity.Core.UI
{
    /// <summary>
    /// Hệ thống quản lý toàn diện các UI Panel / Screen trong ứng dụng.
    /// Tự động khởi tạo Canvas gốc, cấu hình CanvasScaler responsive theo độ phân giải màn hình,
    /// nạp Panels từ Resources/PanelConfig và hỗ trợ điều hướng stack.
    /// </summary>
    public static class UIManager
    {
        public static GameObject UIRoot { get; private set; }
        public static Canvas RootCanvas { get; private set; }
        public static CanvasScaler RootCanvasScaler { get; private set; }

        private static PanelConfig _config;
        private static readonly Dictionary<string, BaseUI> Panels = new Dictionary<string, BaseUI>();
        public static BaseUI PrevUI { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInit()
        {
            _config = Resources.Load<PanelConfig>("PanelConfig");
            if (_config != null)
            {
                EnsureRootCanvas();
                InstantiateConfigPanels();
            }
        }

        public static void EnsureRootCanvas()
        {
            if (UIRoot != null) return;

            UIRoot = new GameObject("[Unity.Core.UIRoot]");
            UnityEngine.Object.DontDestroyOnLoad(UIRoot);

            RootCanvas = UIRoot.AddComponent<Canvas>();
            RootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            RootCanvas.sortingOrder = 10;

            RootCanvasScaler = UIRoot.AddComponent<CanvasScaler>();
            RootCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                RootCanvasScaler.referenceResolution = new Vector2(1920, 1080);
            }
            else
            {
                RootCanvasScaler.referenceResolution = new Vector2(1080, 1920);
            }

            RootCanvasScaler.matchWidthOrHeight = 0.5f;
            UIRoot.AddComponent<GraphicRaycaster>();
        }

        private static void InstantiateConfigPanels()
        {
            if (_config == null || _config.Panels == null) return;

            foreach (var panelPrefab in _config.Panels)
            {
                if (panelPrefab == null) continue;

                string key = panelPrefab.GetType().Name;
                if (!Panels.ContainsKey(key))
                {
                    var instance = UnityEngine.Object.Instantiate(panelPrefab, UIRoot.transform);
                    instance.name = panelPrefab.name;
                    instance.gameObject.SetActive(false);
                    Panels[key] = instance;
                }
            }

            AppLogger.Log($"[UIManager] Initialized {Panels.Count} UI Panels from PanelConfig.");
        }

        public static T ShowPanel<T>(float duration = 0.3f) where T : BaseUI
        {
            EnsureRootCanvas();
            string key = typeof(T).Name;

            if (!Panels.TryGetValue(key, out var panel) || panel == null)
            {
                // Try to find prefab in config or load from resources
                T prefab = GetPrefabFromConfig<T>();
                if (prefab != null)
                {
                    panel = UnityEngine.Object.Instantiate(prefab, UIRoot.transform);
                    panel.name = prefab.name;
                    Panels[key] = panel;
                }
                else
                {
                    var resPrefab = Resources.Load<T>($"UI/{key}");
                    if (resPrefab != null)
                    {
                        panel = UnityEngine.Object.Instantiate(resPrefab, UIRoot.transform);
                        panel.name = resPrefab.name;
                        Panels[key] = panel;
                    }
                    else
                    {
                        AppLogger.LogWarning($"[UIManager] Không tìm thấy Panel của type '{key}'.");
                        return null;
                    }
                }
            }

            if (panel.transform.parent != UIRoot.transform)
            {
                panel.transform.SetParent(UIRoot.transform, false);
            }

            panel.transform.SetAsLastSibling();
            panel.SetInfo();
            panel.Show(duration);
            return panel as T;
        }

        public static void HidePanel<T>(float duration = 0.2f) where T : BaseUI
        {
            string key = typeof(T).Name;
            if (Panels.TryGetValue(key, out var panel) && panel != null)
            {
                PrevUI = panel;
                panel.Hide(duration);
            }
        }

        public static void HideAllPanels()
        {
            foreach (var kvp in Panels)
            {
                if (kvp.Value != null && kvp.Value.gameObject.activeSelf)
                {
                    kvp.Value.Hide(0.1f);
                }
            }
        }

        public static T GetPanel<T>() where T : BaseUI
        {
            string key = typeof(T).Name;
            if (Panels.TryGetValue(key, out var panel))
            {
                return panel as T;
            }
            return null;
        }

        private static T GetPrefabFromConfig<T>() where T : BaseUI
        {
            if (_config == null || _config.Panels == null) return null;
            foreach (var p in _config.Panels)
            {
                if (p is T typed) return typed;
            }
            return null;
        }
    }
}
