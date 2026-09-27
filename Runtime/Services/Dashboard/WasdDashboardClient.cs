using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Core.Logging;

namespace Unity.Core.Services.Dashboard
{
    /// <summary>
    /// HTTP Client kết nối tới Dashboard API:
    /// Endpoint: https://dashboard.wasdmobile.com/api/projects/:id/info
    /// Hỗ trợ xác thực bằng API Key (Header x-api-key và query parameter apiKey).
    /// </summary>
    public class WasdDashboardClient
    {
        public const string DefaultBaseUrl = "";
        public string BaseUrl { get; set; } = DefaultBaseUrl;

        /// <summary>
        /// Gọi API lấy thông tin cấu hình dự án theo Project ID hoặc Mã dự án (Project Code).
        /// </summary>
        /// <param name="projectIdOrCode">ID hoặc Mã của dự án trên Dashboard (ví dụ: 2024-12-AND-Prison-WASD)</param>
        /// <param name="apiKey">API Key xác thực (nếu để trống sẽ tự dùng API Key set cứng trong code)</param>
        /// <param name="timeoutSeconds">Thời gian chờ tối đa (giây)</param>
        public async Task<ProjectInfoData> FetchProjectInfoAsync(string projectIdOrCode, string apiKey = null, int timeoutSeconds = 15)
        {
            if (string.IsNullOrEmpty(projectIdOrCode))
            {
                throw new ArgumentException("Mã hoặc ID dự án không được để trống.", nameof(projectIdOrCode));
            }

            string cleanBaseUrl = BaseUrl?.TrimEnd('/') ?? "";
            if (string.IsNullOrEmpty(cleanBaseUrl))
            {
                AppLogger.Log("[WasdDashboardClient] BaseUrl chưa được cấu hình. Dùng cấu hình cục bộ từ Resources.");
                return null;
            }

            string actualApiKey = string.IsNullOrEmpty(apiKey) ? Unity.Dashboard.DashboardApiClient.HARDCODED_API_KEY : apiKey;
            string url = $"{cleanBaseUrl}/api/projects/{UnityWebRequest.EscapeURL(projectIdOrCode.Trim())}/info";

            // Gắn query param dự phòng
            if (!string.IsNullOrEmpty(actualApiKey))
            {
                url += $"?apiKey={UnityWebRequest.EscapeURL(actualApiKey.Trim())}";
            }

            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = timeoutSeconds;
                request.SetRequestHeader("Accept", "application/json");

                if (!string.IsNullOrEmpty(actualApiKey))
                {
                    request.SetRequestHeader("x-api-key", actualApiKey.Trim());
                }

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

#if UNITY_2020_1_OR_NEWER
                if (request.result != UnityWebRequest.Result.Success)
#else
                if (request.isNetworkError || request.isHttpError)
#endif
                {
                    string errorMsg = $"[WasdDashboardClient] Lỗi gọi API ({request.responseCode}): {request.error}\nURL: {cleanBaseUrl}/api/projects/{projectIdOrCode}/info\nResponse: {request.downloadHandler?.text}";
                    AppLogger.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }

                string responseText = request.downloadHandler.text;
                AppLogger.Log($"[WasdDashboardClient] Tải cấu hình Project thành công từ Dashboard! ({responseText.Length} bytes)");
                return ProjectInfoData.FromJson(responseText);
            }
        }

        /// <summary>
        /// Gọi API lấy thông tin cấu hình dự án (Sử dụng Callback).
        /// </summary>
        public async void FetchProjectInfo(string projectIdOrCode, string apiKey = null, Action<ProjectInfoData> onSuccess = null, Action<string> onError = null, int timeoutSeconds = 15)
        {
            try
            {
                var result = await FetchProjectInfoAsync(projectIdOrCode, apiKey, timeoutSeconds);
                onSuccess?.Invoke(result);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }
    }
}
