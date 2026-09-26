using System;
using Unity.Core.Services.Ads;

namespace Unity.Core.Services.Tracking
{
    /// <summary>
    /// Giao diện cho các hệ thống theo dõi phân bổ và doanh thu quảng cáo (Adjust, AppsFlyer, Firebase, Singular).
    /// </summary>
    public interface ITrackingProvider : IService
    {
        string ProviderName { get; }
        void TrackRevenue(AdRevenueInfo revenueInfo);
        void TrackEvent(string eventToken, double? revenue = null, string currency = null);
    }
}
