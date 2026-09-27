using System;
using Unity.Core.Logging;

namespace Unity.Core.Services.RemoteConfig
{
    /// <summary>
    /// Cổng cấu hình từ xa (Remote Config Service Bridge).
    /// Cho phép truy xuất các tham số cân bằng game động (A/B Testing, Giá tiền, Tỷ lệ rơi đồ) từ xa.
    /// Hoạt động an toàn với giá trị mặc định khi chưa kết nối mạng hoặc chưa cài SDK.
    /// </summary>
    public static class RemoteConfigService
    {
        private static IRemoteConfigProvider _provider;

        /// <summary>
        /// Đăng ký nhà cung cấp cấu hình từ xa (ví dụ: FirebaseRemoteConfigProvider).
        /// </summary>
        public static void Register(IRemoteConfigProvider provider)
        {
            if (provider != null)
            {
                _provider?.Shutdown();
                _provider = provider;
                _provider.Initialize();
                ServiceRegistry.Register(_provider);
                AppLogger.Log($"[RemoteConfigService] Đã đăng ký Remote Config Provider: {_provider.GetType().Name}");
            }
        }

        public static bool IsFetched => _provider != null && _provider.IsFetched;
        public static bool IsRemoteConfigInitialized => IsFetched;
        public static bool IsInitialized => IsFetched;

        /// <summary>
        /// Lấy giá trị cấu hình theo key, tự động trả về defaultValue nếu chưa tải được hoặc chưa có cấu hình.
        /// </summary>
        public static T GetValue<T>(string key, T defaultValue = default)
        {
            if (_provider == null)
            {
                return defaultValue;
            }

            try
            {
                return _provider.GetValue(key, defaultValue);
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"[RemoteConfigService] Lỗi đọc key '{key}': {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Tải cấu hình mới nhất từ máy chủ từ xa.
        /// </summary>
        public static void Fetch(Action<bool> onComplete = null)
        {
            if (_provider == null)
            {
                AppLogger.Log("[RemoteConfigService] Chưa có Provider. Giữ nguyên giá trị mặc định.");
                onComplete?.Invoke(true);
                return;
            }

            _provider.FetchAsync(onComplete);
        }
    }
}
