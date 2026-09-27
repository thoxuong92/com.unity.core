using System;
using UnityEngine;

namespace Unity.Core.Services.Ads
{
    /// <summary>
    /// Cấu hình thời gian hiển thị quảng cáo (Intervals, Caps, Cooldowns).
    /// </summary>
    [Serializable]
    public class AdsSettings
    {
        [Header("General")]
        public bool IsAdsEnabled = true;
        public bool IsRemoveAds = false;

        [Header("Banner")]
        public bool IsBannerEnabled = true;
        public int MaxClickBanner = 5;

        [Header("Interstitial")]
        public bool IsInterstitialEnabled = true;
        public float InterstitialFirstDelay = 30f;
        public float InterstitialInterval = 45f;

        [Header("Rewarded Video")]
        public bool IsRewardedEnabled = true;
        public int MaxRewardedPerDay = 50;

        [Header("App Open Ad")]
        public bool IsAppOpenEnabled = true;
        public float AppOpenInterval = 60f;
        public float TimeLoadAppOpen = 5f;
    }
}
