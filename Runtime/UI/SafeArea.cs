using UnityEngine;
using Unity.Core.Tick;

namespace Unity.Core.UI
{
    /// <summary>
    /// Tự động căn chỉnh RectTransform theo vùng an toàn (Safe Area) của thiết bị (tai thỏ, Dynamic Island, punch-hole camera, thanh điều hướng).
    /// Hỗ trợ giả lập thiết bị trực tiếp trong Unity Editor.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Unity Core/UI/Safe Area")]
    public class SafeArea : TickBehaviour
    {
        public enum SimDevice
        {
            None,
            iPhoneX,
            iPhoneXsMax,
            iPhone14Pro,
            Pixel3XL
        }

        public static SimDevice Sim = SimDevice.None;

        private static readonly Rect[] NsaPresets = new Rect[]
        {
            new Rect(0f, 102f / 2436f, 1f, 2202f / 2436f),     // iPhone X Portrait
            new Rect(132f / 2436f, 63f / 1125f, 2172f / 2436f, 1062f / 1125f), // iPhone X Landscape
            new Rect(0f, 102f / 2688f, 1f, 2454f / 2688f),     // iPhone Xs Max Portrait
            new Rect(132f / 2688f, 63f / 1242f, 2424f / 2688f, 1179f / 1242f), // iPhone Xs Max Landscape
            new Rect(0f, 162f / 2556f, 1f, 2292f / 2556f),     // iPhone 14/15 Pro Portrait (Dynamic Island)
            new Rect(162f / 2556f, 63f / 1179f, 2292f / 2556f, 1116f / 1179f), // iPhone 14/15 Pro Landscape
            new Rect(0f, 0f, 1f, 2789f / 2960f),               // Pixel 3 XL Portrait
            new Rect(171f / 2960f, 0f, 2789f / 2960f, 1f)      // Pixel 3 XL Landscape
        };

        private RectTransform _panel;
        private Rect _lastSafeArea = new Rect(0, 0, 0, 0);
        private Vector2Int _lastScreenSize = new Vector2Int(0, 0);
        private ScreenOrientation _lastOrientation = ScreenOrientation.AutoRotation;

        [SerializeField] private bool conformX = true;
        [SerializeField] private bool conformY = true;
        [SerializeField] private bool logging = false;

        public bool ConformX { get => conformX; set => conformX = value; }
        public bool ConformY { get => conformY; set => conformY = value; }

        protected override void Awake()
        {
            base.Awake();
            _panel = GetComponent<RectTransform>();
            Refresh();
        }

        public override void OnUpdate()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (_panel == null) return;

            Rect safeArea = GetSafeArea();

            if (safeArea != _lastSafeArea ||
                Screen.width != _lastScreenSize.x ||
                Screen.height != _lastScreenSize.y ||
                Screen.orientation != _lastOrientation)
            {
                _lastScreenSize.x = Screen.width;
                _lastScreenSize.y = Screen.height;
                _lastOrientation = Screen.orientation;

                ApplySafeArea(safeArea);
            }
        }

        private Rect GetSafeArea()
        {
            Rect safeArea = Screen.safeArea;

#if UNITY_EDITOR
            if (Sim != SimDevice.None)
            {
                bool isPortrait = Screen.height > Screen.width;
                Rect nsa = Sim switch
                {
                    SimDevice.iPhoneX => isPortrait ? NsaPresets[0] : NsaPresets[1],
                    SimDevice.iPhoneXsMax => isPortrait ? NsaPresets[2] : NsaPresets[3],
                    SimDevice.iPhone14Pro => isPortrait ? NsaPresets[4] : NsaPresets[5],
                    SimDevice.Pixel3XL => isPortrait ? NsaPresets[6] : NsaPresets[7],
                    _ => new Rect(0, 0, 1, 1)
                };

                safeArea = new Rect(
                    Screen.width * nsa.x,
                    Screen.height * nsa.y,
                    Screen.width * nsa.width,
                    Screen.height * nsa.height
                );
            }
#endif
            return safeArea;
        }

        private void ApplySafeArea(Rect r)
        {
            _lastSafeArea = r;

            if (!conformX)
            {
                r.x = 0;
                r.width = Screen.width;
            }

            if (!conformY)
            {
                r.y = 0;
                r.height = Screen.height;
            }

            if (Screen.width > 0 && Screen.height > 0)
            {
                Vector2 anchorMin = r.position;
                Vector2 anchorMax = r.position + r.size;
                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;

                if (!float.IsNaN(anchorMin.x) && !float.IsNaN(anchorMin.y) &&
                    !float.IsNaN(anchorMax.x) && !float.IsNaN(anchorMax.y) &&
                    anchorMin.x >= 0 && anchorMin.y >= 0 && anchorMax.x <= 1.01f && anchorMax.y <= 1.01f)
                {
                    _panel.anchorMin = anchorMin;
                    _panel.anchorMax = anchorMax;
                }
            }

            if (logging)
            {
                Debug.Log($"[SafeArea] Applied to {_panel.name}: x={r.x}, y={r.y}, w={r.width}, h={r.height}");
            }
        }
    }
}
