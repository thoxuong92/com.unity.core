using System;

namespace Unity.Core.Services.Ads
{
    /// <summary>
    /// Thông tin doanh thu quảng cáo (Impression-level ad revenue data) dùng để gửi sang các mạng Tracking (Adjust, AppsFlyer, Firebase).
    /// </summary>
    [Serializable]
    public class AdRevenueInfo
    {
        public string Source { get; set; } = "Max";
        public double Revenue { get; set; } = 0.0;
        public string Currency { get; set; } = "USD";
        public string NetworkName { get; set; } = "";
        
        // Ad Unit identifier (hỗ trợ cả AdUnitId và AdUnitIdentifier)
        public string AdUnitIdentifier { get; set; } = "";
        public string AdUnitId
        {
            get => AdUnitIdentifier;
            set => AdUnitIdentifier = value;
        }

        public string Placement { get; set; } = "";
        public string CountryCode { get; set; } = "";

        // Ad Format (hỗ trợ cả Format và AdFormat)
        public string AdFormat { get; set; } = "";
        public string Format
        {
            get => AdFormat;
            set => AdFormat = value;
        }

        public int ImpressionsCount { get; set; } = 0;
    }
}
