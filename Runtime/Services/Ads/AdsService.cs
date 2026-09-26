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

        public static AdsSettings Settings { get; set; } = new AdsSettings();

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
        public static bool CanShowBanner => Settings.IsAdsEnabled && !Settings.IsRemoveAds && _activeProvider != null && _activeProvider.CanShowBanner;
        public static bool CanShowInterstitial => Settings.IsAdsEnabled && !Settings.IsRemoveAds && _activeProvider != null && _activeProvider.CanShowInterstitial;
        public static bool CanShowRewarded => Settings.IsAdsEnabled && _activeProvider != null && _activeProvider.CanShowRewarded;
        public static bool CanShowAppOpen => Settings.IsAdsEnabled && !Settings.IsRemoveAds && _activeProvider != null && _activeProvider.CanShowAppOpen;

        /// <summary>
        /// Hiển thị Banner quảng cáo.
        /// </summary>
        public static void ShowBanner(string placement = "default_banner")
        {
            if (!CanShowBanner) return;
            _activeProvider?.ShowBanner(placement);
        }

        /// <summary>
        /// Ẩn Banner quảng cáo.
        /// </summary>
        public static void HideBanner()
        {
            _activeProvider?.HideBanner();
        }

        /// <summary>
        /// Hiển thị quảng cáo chuyển cảnh (Interstitial).
        /// </summary>
        public static void ShowInterstitial(string placement, Action onClosed = null)
        {
            if (!CanShowInterstitial)
            {
                onClosed?.Invoke();
                return;
            }

            _activeProvider?.ShowInterstitial(placement, onClosed);
        }

        /// <summary>
        /// Hiển thị quảng cáo trả thưởng (Rewarded Video).
        /// Callback trả về true nếu người chơi xem xong và nhận thưởng, false nếu đóng giữa chừng hoặc lỗi.
        /// </summary>
        public static void ShowRewarded(string placement, Action<bool> onRewardEarned)
        {
            if (!CanShowRewarded)
            {
                onRewardEarned?.Invoke(false);
                return;
            }

            _activeProvider?.ShowRewarded(placement, onRewardEarned);
        }

        /// <summary>
        /// Hiển thị quảng cáo mở ứng dụng (App Open Ad).
        /// </summary>
        public static void ShowAppOpen(string placement, Action onClosed = null)
        {
            if (!CanShowAppOpen)
            {
                onClosed?.Invoke();
                return;
            }

            _activeProvider?.ShowAppOpen(placement, onClosed);
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
