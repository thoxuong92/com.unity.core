using System;
using UnityEngine;

namespace Unity.Core.Services.Ads
{
    /// <summary>
    /// Quảng cáo giả có hình (Editor và bản Development): banner kéo hết chiều ngang ở đáy, khung MREC đúng
    /// chỗ game yêu cầu, và màn quảng cáo toàn màn hình có đếm giờ (App Open, Interstitial, Rewarded).
    /// Dùng để căn UI quanh quảng cáo trước khi có SDK thật. Bật/tắt bằng AdsSettings.MockVisual.
    /// </summary>
    public sealed class MockAdsVisual : MonoBehaviour
    {
        private static MockAdsVisual _instance;

        private string _label;
        private float _elapsed, _duration;
        private bool _rewarded;
        private Action<bool> _done;
        private GUIStyle _style;

        /// <summary>Quảng cáo giả có hình đang được dùng (Editor hoặc bản Development, MockVisual bật).</summary>
        public static bool Enabled => AdsService.Settings.MockVisual && Application.isPlaying && (Application.isEditor || Debug.isDebugBuild);

        /// <summary>Đang hiện một quảng cáo toàn màn hình giả.</summary>
        public static bool IsShowingFullscreen => _instance != null && _instance._done != null;

        private static MockAdsVisual Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[MockAdsVisual]");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<MockAdsVisual>();
                }
                return _instance;
            }
        }

        /// <summary>Bảo đảm overlay tồn tại (để vẽ banner / MREC giả).</summary>
        public static void Ensure()
        {
            if (Enabled) _ = Instance;
        }

        /// <summary>
        /// Một quảng cáo toàn màn hình giả. done(true): xem hết (rewarded: được thưởng); done(false): đóng sớm.
        /// Không bật overlay thì xong ngay với true.
        /// </summary>
        public static void ShowFullscreen(string label, float seconds, bool rewarded, Action<bool> done)
        {
            if (!Enabled) { done?.Invoke(true); return; }
            var i = Instance;
            if (i._done != null) { done?.Invoke(false); return; }
            i._label = label; i._duration = Mathf.Max(0.5f, seconds); i._elapsed = 0f; i._rewarded = rewarded; i._done = done ?? (_ => { });
        }

        private void Update()
        {
            if (_done != null) _elapsed += Time.unscaledDeltaTime;
        }

        private static void Fill(Rect r, Color c) { GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, c, 0, 0); }

        private static void Frame(Rect r, Color c, float t)
        {
            Fill(new Rect(r.x, r.y, r.width, t), c); Fill(new Rect(r.x, r.yMax - t, r.width, t), c);
            Fill(new Rect(r.x, r.y, t, r.height), c); Fill(new Rect(r.xMax - t, r.y, t, r.height), c);
        }

        private void Label(Rect r, string text, int size, Color c)
        {
            _style.fontSize = size; _style.normal.textColor = c;
            GUI.Label(r, text, _style);
        }

        private void OnGUI()
        {
            if (!Enabled) return;
            if (_style == null) _style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, wordWrap = true };
            GUI.depth = -1000;
            GUI.matrix = Matrix4x4.identity;
            float density = AdsService.ScreenDensity;
            var ink = new Color(0.24f, 0.35f, 0.63f);

            // the banner: a strip across the whole bottom of the screen
            if (AdsService.IsBannerShowing)
            {
                float h = Mathf.Max(AdsService.BannerHeightPx, AdsService.BannerReservePx);
                var strip = new Rect(0, Screen.height - h, Screen.width, h);
                Fill(strip, new Color(0.84f, 0.88f, 0.97f, 0.97f));
                float adH = AdsService.BannerHeightPx, adW = AdsService.Settings.MockBannerLarge ? 320f * density : Screen.width;
                var ad = new Rect((Screen.width - adW) / 2f, Screen.height - Mathf.Min(h, adH) - (h - Mathf.Min(h, adH)) / 2f, adW, Mathf.Min(h, adH));
                Fill(ad, new Color(0.93f, 0.95f, 1f));
                Frame(ad, ink, 2f);
                Label(ad, "TEST AD · BANNER " + Mathf.RoundToInt(adW / density) + "×" + Mathf.RoundToInt(ad.height / density), Mathf.RoundToInt(Mathf.Clamp(ad.height * 0.32f, 10, 22)), ink);
            }

            // the MREC where the game asked for it
            if (AdsService.IsMrecShowing)
            {
                var r = AdsService.MrecRect;
                Fill(r, new Color(0.93f, 0.95f, 1f, 0.97f));
                Frame(r, ink, 2f);
                Label(r, "TEST AD · MREC 300×250", Mathf.RoundToInt(14 * density), ink);
            }

            // a full-screen ad
            if (_done != null)
            {
                var full = new Rect(0, 0, Screen.width, Screen.height);
                Fill(full, new Color(0.04f, 0.06f, 0.12f, 0.94f));
                float u = Mathf.Min(Screen.width, Screen.height) / 400f;
                var box = new Rect(Screen.width / 2f - 150 * u, Screen.height / 2f - 80 * u, 300 * u, 160 * u);
                Frame(box, new Color(0.56f, 0.7f, 1f), 3f);
                Label(new Rect(box.x, box.y + 14 * u, box.width, 20 * u), "TEST AD", Mathf.RoundToInt(12 * u), new Color(0.56f, 0.7f, 1f));
                Label(new Rect(box.x, box.y + 44 * u, box.width, 40 * u), _label, Mathf.RoundToInt(22 * u), Color.white);
                int left = Mathf.CeilToInt(_duration - _elapsed);
                bool finished = left <= 0;
                string sub = _rewarded ? (finished ? "✓ Reward earned · close to collect" : "Reward in " + left + "…") : (finished ? "Close to continue" : "Closes in " + left + "…");
                Label(new Rect(box.x, box.y + 96 * u, box.width, 30 * u), sub, Mathf.RoundToInt(13 * u), new Color(1f, 1f, 1f, 0.85f));
                bool canClose = _rewarded || finished;
                var close = new Rect(Screen.width - 60 * u, 14 * u, 44 * u, 44 * u);
                if (canClose)
                {
                    Fill(close, Color.white);
                    Label(close, "✕", Mathf.RoundToInt(22 * u), new Color(0.07f, 0.13f, 0.2f));
                    if (GUI.Button(close, GUIContent.none, GUIStyle.none))
                    {
                        var cb = _done;
                        _done = null;
                        cb(!_rewarded || finished);
                        return;
                    }
                }
                // the game under it takes no taps
                GUI.Button(full, GUIContent.none, GUIStyle.none);
            }
        }
    }
}
