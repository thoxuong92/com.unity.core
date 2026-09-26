using System;
using System.Collections.Generic;
using Unity.Core.Logging;
using Unity.Core.Services.Ads;

namespace Unity.Core.Services.Tracking
{
    /// <summary>
    /// Cổng theo dõi doanh thu và phân bổ quảng cáo (Tracking Service Broadcaster).
    /// Cho phép gọi TrackRevenue ở mọi nơi mà KHÔNG BỊ LỖI dù chưa cài Adjust hay AppsFlyer.
    /// </summary>
    public static class TrackingService
    {
        private static readonly List<ITrackingProvider> Providers = new List<ITrackingProvider>();

        /// <summary>
        /// Thêm một nhà cung cấp Tracking (ví dụ: AdjustTrackingProvider, AppsFlyerTrackingProvider).
        /// </summary>
        public static void AddProvider(ITrackingProvider provider)
        {
            if (provider != null && !Providers.Contains(provider))
            {
                provider.Initialize();
                Providers.Add(provider);
                AppLogger.Log($"[TrackingService] Đã thêm Tracking Provider: {provider.ProviderName}");
            }
        }

        /// <summary>
        /// Theo dõi doanh thu quảng cáo (Impression-level Ad Revenue).
        /// </summary>
        public static void TrackRevenue(AdRevenueInfo revenueInfo)
        {
            if (revenueInfo == null) return;

            if (Providers.Count == 0)
            {
                AppLogger.Log($"[TrackingService:Console] TrackRevenue: {revenueInfo.Revenue} {revenueInfo.Currency} from {revenueInfo.Source} ({revenueInfo.NetworkName})");
                return;
            }

            for (int i = 0; i < Providers.Count; i++)
            {
                try
                {
                    Providers[i].TrackRevenue(revenueInfo);
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"[TrackingService] Lỗi gửi revenue tới '{Providers[i].ProviderName}': {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Theo dõi sự kiện chuyển đổi hoặc doanh thu mua hàng.
        /// </summary>
        public static void TrackEvent(string eventToken, double? revenue = null, string currency = null)
        {
            if (string.IsNullOrEmpty(eventToken)) return;

            if (Providers.Count == 0)
            {
                AppLogger.Log($"[TrackingService:Console] TrackEvent: {eventToken}, Revenue: {revenue} {currency}");
                return;
            }

            for (int i = 0; i < Providers.Count; i++)
            {
                try
                {
                    Providers[i].TrackEvent(eventToken, revenue, currency);
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"[TrackingService] Lỗi gửi event tới '{Providers[i].ProviderName}': {ex.Message}");
                }
            }
        }
    }
}
