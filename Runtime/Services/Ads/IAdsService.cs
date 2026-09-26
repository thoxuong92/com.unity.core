using System;

namespace Unity.Core.Services.Ads
{
    /// <summary>
    /// Giao diện chuẩn cho các hệ thống quảng cáo (Max, AdMob, UnityAds, IronSource, Mock).
    /// </summary>
    public interface IAdsService : IService
    {
        bool IsInitialized { get; }
        bool CanShowBanner { get; }
        bool CanShowInterstitial { get; }
        bool CanShowRewarded { get; }
        bool CanShowAppOpen { get; }

        void ShowBanner(string placement = null);
        void HideBanner();
        void ShowInterstitial(string placement, Action onClosed = null);
        void ShowRewarded(string placement, Action<bool> onRewardEarned);
        void ShowAppOpen(string placement, Action onClosed = null);
    }
}
