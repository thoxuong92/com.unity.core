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
        public bool EnforceCooldowns = true;

        [Header("Banner")]
        public bool IsBannerEnabled = true;
        public int MaxClickBanner = 5;

        [Header("Interstitial")]
        public bool IsInterstitialEnabled = true;
        public float InterstitialFirstDelay = 30f;
        public float InterstitialInterval = 45f;
        public float RewardedInterstitialGrace = 18f;

        [Header("Rewarded Video")]
        public bool IsRewardedEnabled = true;
        public int MaxRewardedPerDay = 50;

        [Header("App Open Ad")]
        public bool IsAppOpenEnabled = true;
        public float AppOpenInterval = 60f;
        public float TimeLoadAppOpen = 5f;

        [Header("MREC (300x250)")]
        public bool IsMrecEnabled = true;

        [Header("Editor / Development mock")]
        [Tooltip("Quảng cáo giả có hình (MockAdsVisual) trong Editor và bản Development, để căn UI")]
        public bool MockVisual = true;
        [Tooltip("Banner giả là large banner 320x100 (mặc định: smart banner rộng hết màn, cao 50 / 90 dp)")]
        public bool MockBannerLarge = false;
        public float MockAppOpenSeconds = 3f;
        public float MockInterstitialSeconds = 3f;
        public float MockRewardedSeconds = 5f;
    }
}
