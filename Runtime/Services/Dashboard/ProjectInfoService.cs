using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Core.Logging;

namespace Unity.Core.Services.Dashboard
{
    /// <summary>
    /// Service Bridge quản lý thông tin cấu hình dự án toàn cục (Project Info Service).
    /// Tự động nạp cấu hình offline từ Resources khi khởi động và hỗ trợ cập nhật động từ WASD Dashboard API.
    /// </summary>
    public static class ProjectInfoService
    {
        private static ProjectInfoData _currentInfo = new ProjectInfoData();
        public static ProjectInfoData CurrentInfo => _currentInfo;

        public static bool IsFetched { get; private set; }

        private static WasdDashboardClient _client = new WasdDashboardClient();
        public static WasdDashboardClient Client => _client;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoLoad()
        {
            LoadFromResources();
        }

        /// <summary>
        /// Nạp thông tin cấu hình từ Resources (Assets/Resources/Info.json hoặc Info_Android/Info_iOS).
        /// </summary>
        public static void LoadFromResources()
        {
            try
            {
                string platformSuffix = Application.platform == RuntimePlatform.Android ? "_Android" : (Application.platform == RuntimePlatform.IPhonePlayer ? "_iOS" : "");
                TextAsset data = Resources.Load<TextAsset>($"Info{platformSuffix}") ?? Resources.Load<TextAsset>("Info");

                if (data != null && !string.IsNullOrEmpty(data.text))
                {
                    _currentInfo = ProjectInfoData.FromJson(data.text);
                    AppLogger.Log($"[ProjectInfoService] Đã nạp cấu hình offline từ Resources ({data.name}).");
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"[ProjectInfoService] Không thể đọc cấu hình từ Resources: {ex.Message}");
            }
        }

        /// <summary>
        /// Đồng bộ thông tin dự án mới nhất từ WASD Dashboard API theo Project ID hoặc Mã dự án (Project Code).
        /// </summary>
        public static async Task<bool> FetchAsync(string projectIdOrCode, string apiKey = null, int timeoutSeconds = 15)
        {
            try
            {
                var data = await _client.FetchProjectInfoAsync(projectIdOrCode, apiKey, timeoutSeconds);
                if (data != null)
                {
                    _currentInfo = data;
                    IsFetched = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"[ProjectInfoService] Đồng bộ Dashboard thất bại: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Đồng bộ thông tin dự án mới nhất từ WASD Dashboard API (Callback).
        /// </summary>
        public static async void Fetch(string projectIdOrCode, string apiKey = null, Action<bool> onComplete = null, int timeoutSeconds = 15)
        {
            bool success = await FetchAsync(projectIdOrCode, apiKey, timeoutSeconds);
            onComplete?.Invoke(success);
        }

        #region Quick Accessors
        public static string Name => _currentInfo?.Name ?? "";
        public static string Code => _currentInfo?.Code ?? "";
        public static string Platform => _currentInfo?.Platform ?? "";
        public static string PackageName => _currentInfo?.PackageName ?? "";
        public static string AppleId => _currentInfo?.AppleId ?? "";
        public static string Version => _currentInfo?.Version ?? "1.0.0";

        // Ads
        public static string SDKMax => _currentInfo?.SDKMax ?? "";
        public static string AdMobAppId => _currentInfo?.AdMobAppId ?? "";
        public static string AppOpen => _currentInfo?.AppOpen ?? "";
        public static string Banner => _currentInfo?.Banner ?? "";
        public static string Interstitial => _currentInfo?.Interstitial ?? "";
        public static string Rewarded => _currentInfo?.Rewarded ?? "";
        public static string Mrec => _currentInfo?.Mrec ?? "";
        public static string BannerPosition => _currentInfo?.BannerPosition ?? "BottomCenter";

        // Tracking
        public static string TrackingPlatform => _currentInfo?.TrackingPlatform ?? "";
        public static string AdjustToken => _currentInfo?.AdjustToken ?? "";
        public static string AdjustEnvironment => _currentInfo?.AdjustEnvironment ?? "Production";
        public static string AppsflyerDevKey => _currentInfo?.AppsflyerDevKey ?? "";
        public static string AppsflyerAppId => _currentInfo?.AppsflyerAppId ?? "";
        public static string FirebaseProjectId => _currentInfo?.FirebaseProjectId ?? "";

        // Legal
        public static string PrivacyPolicy => _currentInfo?.PrivacyPolicy ?? "";
        public static string TermsOfUse => _currentInfo?.TermsOfUse ?? "";
        public static string AppAdsTxt => _currentInfo?.AppAdsTxt ?? "";
        #endregion
    }
}
