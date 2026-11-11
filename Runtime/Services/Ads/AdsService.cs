using System;
using UnityEngine;
using Unity.Core.Logging;

namespace Unity.Core.Services.Ads
{
    /// <summary>
    /// Cổng giao tiếp quảng cáo trung tâm (Ads Service Bridge).
    /// Cho phép gọi quảng cáo ở mọi nơi trong gameplay/UI mà KHÔNG BỊ LỖI dù chưa cài đặt SDK Ads (sử dụng Mock Provider mặc định).
    /// Khi cài đặt package Ads (Max, AdMob...), adapter sẽ tự động thay thế và kích hoạt quảng cáo thật.
    /// </summary>
    public static class AdsService
    {
        private static IAdsService _activeProvider = new MockAdsProvider();
        private static float _sessionBootTime;
        private static float _lastInterstitialShownTime = -9999f;
        private static float _lastRewardedCompletedTime = -9999f;
        private static float _lastAppOpenShownTime = -9999f;
        private static float? _simulatedNowSeconds;

        public static AdsSettings Settings { get; set; } = new AdsSettings();

        public static bool IsBannerShowing { get; private set; }
        public static string ActiveBannerPlacement { get; private set; } = "";
        public static int SessionInterstitialCount { get; private set; }
        public static int SessionRewardedCount { get; private set; }
        public static int SessionAppOpenCount { get; private set; }

        /// <summary>
        /// Sự kiện thông báo trạng thái hiển thị của Ad Banner (isVisible, placement).
        /// Cho phép UI điều chỉnh Safe Area hoặc hiển thị thanh Banner Mock trong Editor.
        /// </summary>
        public static event Action<bool, string> OnBannerVisibilityChanged;

        private static float CurrentTime => _simulatedNowSeconds ?? Time.realtimeSinceStartup;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitSubsystemState()
        {
            ResetSessionState();
        }

        /// <summary>
        /// Đặt lại bộ đếm phiên và thời gian Cooldown (hỗ trợ Enter Play Mode không reload domain hoặc Unit Test).
        /// </summary>
        public static void ResetSessionState()
        {
            _simulatedNowSeconds = null;
            _sessionBootTime = Time.realtimeSinceStartup;
            _lastInterstitialShownTime = -9999f;
            _lastRewardedCompletedTime = -9999f;
            _lastAppOpenShownTime = -9999f;
            IsBannerShowing = false;
            ActiveBannerPlacement = "";
            SessionInterstitialCount = 0;
            SessionRewardedCount = 0;
            SessionAppOpenCount = 0;
            ShowedFirstOpen = false;
            ShowFirstOpenDone = false;
        }

        /// <summary>
        /// Ghi đè đồng hồ thời gian thực phục vụ kiểm thử tự động Cooldown (truyền null để quay về Time.realtimeSinceStartup).
        /// </summary>
        public static void SetSimulatedTimeForTesting(float? seconds)
        {
            _simulatedNowSeconds = seconds;
        }

        /// <summary>
        /// Đăng ký adapter quảng cáo từ bên thứ 3 (ví dụ: MaxAdsService, AdMobAdsService).
        /// </summary>
        public static void Register(IAdsService adsProvider)
        {
            if (adsProvider != null)
            {
                _activeProvider?.Shutdown();
                _activeProvider = adsProvider;
                _activeProvider.Initialize();
                ServiceRegistry.Register(_activeProvider);
                AppLogger.Log($"[AdsService] Đã đăng ký nhà cung cấp quảng cáo: {_activeProvider.GetType().Name}");
            }
        }

        public static bool IsInitialized => _activeProvider != null && _activeProvider.IsInitialized;
        public static bool CanShowBanner => Settings.IsAdsEnabled && Settings.IsBannerEnabled && !Settings.IsRemoveAds && _activeProvider != null && _activeProvider.CanShowBanner;
        public static bool CanShowInterstitial => Settings.IsAdsEnabled && Settings.IsInterstitialEnabled && !Settings.IsRemoveAds && _activeProvider != null && _activeProvider.CanShowInterstitial;
        public static bool CanShowRewarded => Settings.IsAdsEnabled && Settings.IsRewardedEnabled && _activeProvider != null && _activeProvider.CanShowRewarded && (Settings.MaxRewardedPerDay <= 0 || SessionRewardedCount < Settings.MaxRewardedPerDay);
        public static bool CanShowAppOpen => Settings.IsAdsEnabled && Settings.IsAppOpenEnabled && !Settings.IsRemoveAds && _activeProvider != null && _activeProvider.CanShowAppOpen;

        /// <summary>
        /// Thời gian chờ còn lại (giây) trước khi được phép hiển thị Interstitial tiếp theo
        /// (tính đủ InterstitialFirstDelay, InterstitialInterval và RewardedInterstitialGrace).
        /// </summary>
        public static float GetInterstitialCooldownRemaining()
        {
            if (!Settings.EnforceCooldowns) return 0f;
            float now = CurrentTime;
            float waitFirst = Mathf.Max(0f, (_sessionBootTime + Settings.InterstitialFirstDelay) - now);
            float waitInterval = _lastInterstitialShownTime > -1000f
                ? Mathf.Max(0f, (_lastInterstitialShownTime + Settings.InterstitialInterval) - now)
                : 0f;
            float waitRewardedGrace = _lastRewardedCompletedTime > -1000f
                ? Mathf.Max(0f, (_lastRewardedCompletedTime + Settings.RewardedInterstitialGrace) - now)
                : 0f;
            return Mathf.Max(waitFirst, Mathf.Max(waitInterval, waitRewardedGrace));
        }

        public static bool IsInterstitialCooldownReady => GetInterstitialCooldownRemaining() <= 0.001f;

        /// <summary>
        /// Thời gian chờ còn lại (giây) trước khi được phép hiển thị App Open Ad khi resume.
        /// </summary>
        public static float GetAppOpenCooldownRemaining()
        {
            if (!Settings.EnforceCooldowns || _lastAppOpenShownTime <= -1000f) return 0f;
            return Mathf.Max(0f, (_lastAppOpenShownTime + Settings.AppOpenInterval) - CurrentTime);
        }

        public static bool IsAppOpenCooldownReady => GetAppOpenCooldownRemaining() <= 0.001f;

        /// <summary>
        /// Bật/tắt gói Remove Ads (tự động ẩn Banner ngay lập tức nếu đang hiển thị).
        /// </summary>
        public static void SetRemoveAds(bool removeAds)
        {
            Settings.IsRemoveAds = removeAds;
            if (removeAds)
            {
                HideBanner();
            }
        }

        /// <summary>
        /// Hiển thị Banner quảng cáo.
        /// </summary>
        public static void ShowBanner(string placement = "default_banner")
        {
            if (!CanShowBanner)
            {
                if (IsBannerShowing) HideBanner();
                return;
            }

            string resolved = string.IsNullOrEmpty(placement) ? "default_banner" : placement;
            _activeProvider?.ShowBanner(resolved);
            IsBannerShowing = true;
            ActiveBannerPlacement = resolved;
            OnBannerVisibilityChanged?.Invoke(true, resolved);
        }

        /// <summary>
        /// Ẩn Banner quảng cáo.
        /// </summary>
        public static void HideBanner()
        {
            _activeProvider?.HideBanner();
            bool wasShowing = IsBannerShowing;
            IsBannerShowing = false;
            if (wasShowing)
            {
                OnBannerVisibilityChanged?.Invoke(false, ActiveBannerPlacement);
            }
        }

        /// <summary>
        /// Thử hiển thị quảng cáo chuyển cảnh (Interstitial) có kiểm tra Cooldown & Grace sau Rewarded Video.
        /// Trả về true nếu quảng cáo thực sự được gọi hiển thị, false nếu bị chặn bởi cài đặt hoặc Cooldown.
        /// </summary>
        public static bool TryShowInterstitial(string placement, Action onClosed = null)
        {
            if (!CanShowInterstitial || !IsInterstitialCooldownReady)
            {
                onClosed?.Invoke();
                return false;
            }

            _lastInterstitialShownTime = CurrentTime;
            SessionInterstitialCount++;
            _activeProvider?.ShowInterstitial(placement, () =>
            {
                _lastInterstitialShownTime = CurrentTime;
                onClosed?.Invoke();
            });
            return true;
        }

        /// <summary>
        /// Hiển thị quảng cáo chuyển cảnh (Interstitial) tuân thủ cấu hình Cooldown trong AdsSettings.
        /// </summary>
        public static void ShowInterstitial(string placement, Action onClosed = null)
        {
            TryShowInterstitial(placement, onClosed);
        }

        /// <summary>
        /// Hiển thị quảng cáo trả thưởng (Rewarded Video).
        /// Callback trả về true nếu người chơi xem xong và nhận thưởng, đồng thời kích hoạt RewardedInterstitialGrace.
        /// </summary>
        public static void ShowRewarded(string placement, Action<bool> onRewardEarned)
        {
            if (!CanShowRewarded)
            {
                onRewardEarned?.Invoke(false);
                return;
            }

            _activeProvider?.ShowRewarded(placement, earned =>
            {
                if (earned)
                {
                    SessionRewardedCount++;
                    _lastRewardedCompletedTime = CurrentTime;
                }
                onRewardEarned?.Invoke(earned);
            });
        }

        public static bool ShowedFirstOpen { get; set; } = false;
        public static bool ShowFirstOpenDone { get; set; } = false;

        /// <summary>
        /// Thử hiển thị quảng cáo mở ứng dụng (App Open Ad) có kiểm tra AppOpenInterval.
        /// </summary>
        public static bool TryShowAppOpen(string placement, Action onClosed = null)
        {
            bool bypassInterval = string.Equals(placement, "first_open", StringComparison.OrdinalIgnoreCase);
            if (!CanShowAppOpen || (!bypassInterval && !IsAppOpenCooldownReady))
            {
                onClosed?.Invoke();
                return false;
            }

            _lastAppOpenShownTime = CurrentTime;
            SessionAppOpenCount++;
            _activeProvider?.ShowAppOpen(placement, () =>
            {
                _lastAppOpenShownTime = CurrentTime;
                onClosed?.Invoke();
            });
            return true;
        }

        /// <summary>
        /// Hiển thị quảng cáo mở ứng dụng (App Open Ad).
        /// </summary>
        public static void ShowAppOpen(string placement, Action onClosed = null)
        {
            TryShowAppOpen(placement, onClosed);
        }

        /// <summary>
        /// Gọi hiển thị App Open Ad đầu tiên khi mở game / splash screen.
        /// Tự động cập nhật cờ ShowedFirstOpen và ShowFirstOpenDone khi quảng cáo kết thúc hoặc bị đóng.
        /// </summary>
        public static void OnAppOpenLoaded(Action onClosed = null)
        {
            if (ShowedFirstOpen)
            {
                onClosed?.Invoke();
                return;
            }

            ShowedFirstOpen = true;

            if (!CanShowAppOpen)
            {
                ShowFirstOpenDone = true;
                onClosed?.Invoke();
                return;
            }

            ShowAppOpen("first_open", () =>
            {
                ShowFirstOpenDone = true;
                onClosed?.Invoke();
            });
        }

        #region Mock Fallback Provider

        /// <summary>
        /// Provider giả lập mặc định giúp game chạy trơn tru trong Unity Editor và khi chưa cài SDK.
        /// </summary>
        private class MockAdsProvider : IAdsService
        {
            public bool IsInitialized => true;
            public bool CanShowBanner => true;
            public bool CanShowInterstitial => true;
            public bool CanShowRewarded => true;
            public bool CanShowAppOpen => true;

            public void Initialize()
            {
                AppLogger.Log("[AdsService] Initialized with MockAdsProvider (Chưa có package Ads bên thứ 3).");
            }

            public void Shutdown() { }

            public void ShowBanner(string placement = null)
            {
                AppLogger.Log($"[AdsService:Mock] ShowBanner ({placement ?? "default"})");
            }

            public void HideBanner()
            {
                AppLogger.Log("[AdsService:Mock] HideBanner");
            }

            public void ShowInterstitial(string placement, Action onClosed = null)
            {
                AppLogger.Log($"[AdsService:Mock] ShowInterstitial ('{placement}') -> Hoàn tất ngay lập tức.");
                onClosed?.Invoke();
            }

            public void ShowRewarded(string placement, Action<bool> onRewardEarned)
            {
                AppLogger.Log($"[AdsService:Mock] ShowRewarded ('{placement}') -> Tự động trả thưởng (Reward Earned = true).");
                onRewardEarned?.Invoke(true);
            }

            public void ShowAppOpen(string placement, Action onClosed = null)
            {
                AppLogger.Log($"[AdsService:Mock] ShowAppOpen ('{placement}') -> Hoàn tất.");
                onClosed?.Invoke();
            }
        }

        #endregion
    }
}
