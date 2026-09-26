using System.Collections.Generic;

namespace Unity.Core.Services.Analytics
{
    /// <summary>
    /// Giao diện cho các nhà cung cấp phân tích người dùng (Firebase, GameAnalytics, AppsFlyer, Facebook Analytics, Mock).
    /// </summary>
    public interface IAnalyticsProvider : IService
    {
        string ProviderName { get; }
        void LogEvent(string eventName, Dictionary<string, object> parameters = null);
        void SetUserProperty(string propertyName, string propertyValue);
        void SetUserId(string userId);
    }
}
