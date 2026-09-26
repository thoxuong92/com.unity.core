using System;
using System.Collections.Generic;
using System.Text;
using Unity.Core.Logging;

namespace Unity.Core.Services.Analytics
{
    /// <summary>
    /// Cổng phân tích dữ liệu người dùng (Analytics Service Broadcaster).
    /// Cho phép gọi LogEvent ở mọi nơi mà KHÔNG BỊ LỖI dù chưa cài Firebase / GameAnalytics (tự động in Console log).
    /// Hỗ trợ gửi đồng thời tới nhiều Provider cùng lúc (ví dụ: Firebase + GameAnalytics).
    /// </summary>
    public static class AnalyticsService
    {
        private static readonly List<IAnalyticsProvider> Providers = new List<IAnalyticsProvider>();

        /// <summary>
        /// Thêm một nhà cung cấp phân tích (ví dụ: FirebaseAnalyticsProvider).
        /// </summary>
        public static void AddProvider(IAnalyticsProvider provider)
        {
            if (provider != null && !Providers.Contains(provider))
            {
                provider.Initialize();
                Providers.Add(provider);
                AppLogger.Log($"[AnalyticsService] Đã thêm Analytics Provider: {provider.ProviderName}");
            }
        }

        /// <summary>
        /// Gửi sự kiện phân tích kèm tham số dữ liệu.
        /// </summary>
        public static void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (string.IsNullOrEmpty(eventName)) return;

            if (Providers.Count == 0)
            {
                // Fallback in Console khi chưa cài package
                string paramsSummary = FormatParams(parameters);
                AppLogger.Log($"[AnalyticsService:Console] LogEvent: '{eventName}' {paramsSummary}");
                return;
            }

            for (int i = 0; i < Providers.Count; i++)
            {
                try
                {
                    Providers[i].LogEvent(eventName, parameters);
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"[AnalyticsService] Lỗi gửi event tới '{Providers[i].ProviderName}': {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Gửi sự kiện đơn giản không có tham số.
        /// </summary>
        public static void LogEvent(string eventName)
        {
            LogEvent(eventName, null);
        }

        /// <summary>
        /// Gán thuộc tính người chơi (User Property).
        /// </summary>
        public static void SetUserProperty(string name, string value)
        {
            if (Providers.Count == 0)
            {
                AppLogger.Log($"[AnalyticsService:Console] SetUserProperty: {name} = {value}");
                return;
            }

            for (int i = 0; i < Providers.Count; i++)
            {
                Providers[i].SetUserProperty(name, value);
            }
        }

        /// <summary>
        /// Gán định danh người chơi (User ID).
        /// </summary>
        public static void SetUserId(string userId)
        {
            if (Providers.Count == 0)
            {
                AppLogger.Log($"[AnalyticsService:Console] SetUserId: {userId}");
                return;
            }

            for (int i = 0; i < Providers.Count; i++)
            {
                Providers[i].SetUserId(userId);
            }
        }

        private static string FormatParams(Dictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0) return "{}";

            var sb = new StringBuilder("{ ");
            foreach (var kvp in parameters)
            {
                sb.Append($"{kvp.Key}: {kvp.Value}, ");
            }
            if (sb.Length > 2) sb.Length -= 2;
            sb.Append(" }");
            return sb.ToString();
        }
    }
}
