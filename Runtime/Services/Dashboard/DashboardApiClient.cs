using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Core.Logging;

namespace Unity.Dashboard
{
    /// <summary>
    /// API Client kết nối trực tiếp tới WASD Mobile Dashboard để lấy cấu hình Marketing / Ads / Tracking theo Mã dự án (Project Code).
    /// </summary>
    public static class DashboardApiClient
    {
        // -----------------------------------------------------------------------------------------
        // 🔑 CẤU HÌNH CỨNG TRONG CODE (Hardcoded Configurations)
        // -----------------------------------------------------------------------------------------
        public const string DEFAULT_SERVER_URL = "https://dashboard.wasdmobile.com";
        
        /// <summary>
        /// API Key được set cứng trong code. Bạn có thể thay đổi trực tiếp chuỗi này.
        /// </summary>
        public const string HARDCODED_API_KEY = "wm_9a5d319c80fe164677ae5cddf1405624c5ba78e2288fda2f";

        // -----------------------------------------------------------------------------------------
        // 1. ASYNC / AWAIT APIS
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Lấy thông tin dự án bất đồng bộ bằng Mã dự án (Tự động dùng Server URL và API Key mặc định).
        /// </summary>
        /// <param name="projectCode">Mã dự án (ví dụ: 2024-12-AND-Prison-WASD)</param>
        public static Task<ProjectMktData> FetchProjectInfoAsync(string projectCode)
        {
            return FetchProjectInfoAsync(DEFAULT_SERVER_URL, projectCode, HARDCODED_API_KEY);
        }

        /// <summary>
        /// Lấy thông tin dự án bất đồng bộ theo Server URL và Mã dự án (Dùng API Key mặc định).
        /// </summary>
        /// <param name="serverUrl">Địa chỉ máy chủ Dashboard</param>
        /// <param name="projectCode">Mã dự án</param>
        public static Task<ProjectMktData> FetchProjectInfoAsync(string serverUrl, string projectCode)
        {
            return FetchProjectInfoAsync(serverUrl, projectCode, HARDCODED_API_KEY);
        }

        /// <summary>
        /// Lấy thông tin dự án bất đồng bộ đầy đủ tham số (Hỗ trợ override API Key nếu cần).
        /// </summary>
        public static async Task<ProjectMktData> FetchProjectInfoAsync(string serverUrl, string projectCode, string apiKey, int timeoutSeconds = 15)
        {
            if (string.IsNullOrEmpty(projectCode))
            {
                Debug.LogError("[DashboardApiClient] Mã dự án (projectCode) không được để trống!");
                return null;
            }

            string actualServerUrl = string.IsNullOrEmpty(serverUrl) ? DEFAULT_SERVER_URL : serverUrl.TrimEnd('/');
            string actualApiKey = string.IsNullOrEmpty(apiKey) ? HARDCODED_API_KEY : apiKey.Trim();

            string endpoint = $"{actualServerUrl}/api/projects/{UnityWebRequest.EscapeURL(projectCode.Trim())}/info";
            if (!string.IsNullOrEmpty(actualApiKey))
            {
                endpoint += $"?apiKey={UnityWebRequest.EscapeURL(actualApiKey)}";
            }

            using (UnityWebRequest request = UnityWebRequest.Get(endpoint))
            {
                request.timeout = timeoutSeconds;
                request.SetRequestHeader("Accept", "application/json");

                if (!string.IsNullOrEmpty(actualApiKey))
                {
                    request.SetRequestHeader("x-api-key", actualApiKey);
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
                    string errorMsg = $"[DashboardApiClient] Lỗi ({request.responseCode}): {request.error}\nURL: {endpoint}\nResponse: {request.downloadHandler?.text}";
                    Debug.LogError(errorMsg);
                    return null;
                }

                string json = request.downloadHandler.text;
                return ProjectMktData.FromJson(json);
            }
        }

        // -----------------------------------------------------------------------------------------
        // 2. COROUTINE APIS (Dùng với StartCoroutine)
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Coroutine lấy thông tin dự án (Dùng Server URL và API Key mặc định).
        /// </summary>
        public static IEnumerator FetchProjectInfoCoroutine(
            string projectCode,
            Action<ProjectMktData> onSuccess,
            Action<string> onError = null,
            int timeoutSeconds = 15)
        {
            return FetchProjectInfoCoroutine(DEFAULT_SERVER_URL, projectCode, HARDCODED_API_KEY, onSuccess, onError, timeoutSeconds);
        }

        /// <summary>
        /// Coroutine lấy thông tin dự án theo Server URL và Mã dự án (Dùng API Key mặc định).
        /// </summary>
        public static IEnumerator FetchProjectInfoCoroutine(
            string serverUrl,
            string projectCode,
            Action<ProjectMktData> onSuccess,
            Action<string> onError = null,
            int timeoutSeconds = 15)
        {
            return FetchProjectInfoCoroutine(serverUrl, projectCode, HARDCODED_API_KEY, onSuccess, onError, timeoutSeconds);
        }

        /// <summary>
        /// Coroutine lấy thông tin dự án đầy đủ tham số.
        /// </summary>
        public static IEnumerator FetchProjectInfoCoroutine(
            string serverUrl,
            string projectCode,
            string apiKey,
            Action<ProjectMktData> onSuccess,
            Action<string> onError = null,
            int timeoutSeconds = 15)
        {
            if (string.IsNullOrEmpty(projectCode))
            {
                onError?.Invoke("Mã dự án (projectCode) không được để trống!");
                yield break;
            }

            string actualServerUrl = string.IsNullOrEmpty(serverUrl) ? DEFAULT_SERVER_URL : serverUrl.TrimEnd('/');
            string actualApiKey = string.IsNullOrEmpty(apiKey) ? HARDCODED_API_KEY : apiKey.Trim();

            string endpoint = $"{actualServerUrl}/api/projects/{UnityWebRequest.EscapeURL(projectCode.Trim())}/info";
            if (!string.IsNullOrEmpty(actualApiKey))
            {
                endpoint += $"?apiKey={UnityWebRequest.EscapeURL(actualApiKey)}";
            }

            using (UnityWebRequest request = UnityWebRequest.Get(endpoint))
            {
                request.timeout = timeoutSeconds;
                request.SetRequestHeader("Accept", "application/json");

                if (!string.IsNullOrEmpty(actualApiKey))
                {
                    request.SetRequestHeader("x-api-key", actualApiKey);
                }

                yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (request.result != UnityWebRequest.Result.Success)
#else
                if (request.isNetworkError || request.isHttpError)
#endif
                {
                    string errorMsg = $"Lỗi HTTP ({request.responseCode}): {request.error} | {request.downloadHandler?.text}";
                    onError?.Invoke(errorMsg);
                }
                else
                {
                    string json = request.downloadHandler.text;
                    ProjectMktData data = ProjectMktData.FromJson(json);
                    onSuccess?.Invoke(data);
                }
            }
        }
    }
}
