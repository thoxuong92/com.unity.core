using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Core.Services.Dashboard;

namespace Unity.Dashboard
{
    /// <summary>
    /// Model dữ liệu cấu hình dự án nhận từ Dashboard API.
    /// Tự động bóc tách từ block "data" và hỗ trợ đầy đủ các trường Ads, Tracking, Legal & Mediation Networks.
    /// </summary>
    [Serializable]
    public class ProjectMktData
    {
        public string id;
        public string code;
        public string name;
        public string platform;
        public string packageName;
        public string appleId;
        public string version;

        public MaxConfig max = new MaxConfig();
        public TrackingConfig tracking = new TrackingConfig();
        public LegalConfig legal = new LegalConfig();
        public List<MediationNetworkInfo> mediationNetworks = new List<MediationNetworkInfo>();

        [NonSerialized]
        public string rawJson;

        public static ProjectMktData FromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;

            try
            {
                // Nếu JSON được bọc trong { "ok": true, "data": { ... } }
                string targetJson = json;
                if (json.Contains("\"data\""))
                {
                    int dataIdx = json.IndexOf("\"data\"", StringComparison.Ordinal);
                    int openBrace = json.IndexOf('{', dataIdx);
                    if (openBrace >= 0)
                    {
                        int closeBrace = FindMatchingBrace(json, openBrace);
                        if (closeBrace > openBrace)
                        {
                            targetJson = json.Substring(openBrace, closeBrace - openBrace + 1);
                        }
                    }
                }

                ProjectMktData data = JsonUtility.FromJson<ProjectMktData>(targetJson);
                if (data == null) data = new ProjectMktData();
                data.rawJson = json;

                if (data.max == null) data.max = new MaxConfig();
                if (data.tracking == null) data.tracking = new TrackingConfig();
                if (data.legal == null) data.legal = new LegalConfig();

                PopulateFallbacks(data, targetJson);
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ProjectMktData] JSON Parsing warning: {ex.Message}");
                return new ProjectMktData { rawJson = json };
            }
        }

        private static void PopulateFallbacks(ProjectMktData data, string json)
        {
            if (string.IsNullOrEmpty(data.id)) data.id = Extract(json, "id", "_id", "projectId");
            if (string.IsNullOrEmpty(data.code)) data.code = Extract(json, "code", "projectCode", "project_code");
            if (string.IsNullOrEmpty(data.name)) data.name = Extract(json, "name", "projectName", "title");
            if (string.IsNullOrEmpty(data.platform)) data.platform = Extract(json, "platform");
            if (string.IsNullOrEmpty(data.packageName)) data.packageName = Extract(json, "packageName", "bundleId", "appId");
            if (string.IsNullOrEmpty(data.appleId)) data.appleId = Extract(json, "appleId", "apple_id", "appStoreId");

            // Max fallbacks
            if (string.IsNullOrEmpty(data.max.sdkKey)) data.max.sdkKey = Extract(json, "sdkKey", "SDKMax", "sdkMax", "applovinSdkKey");
            if (string.IsNullOrEmpty(data.max.admobAppId)) data.max.admobAppId = Extract(json, "admobAppId", "admob_app_id");
            if (string.IsNullOrEmpty(data.max.aoa)) data.max.aoa = Extract(json, "aoa", "appOpen", "AppOpen", "appOpenId");
            if (string.IsNullOrEmpty(data.max.inter)) data.max.inter = Extract(json, "inter", "interstitial", "Interstitial", "interstitialId");
            if (string.IsNullOrEmpty(data.max.rewarded)) data.max.rewarded = Extract(json, "rewarded", "Rewarded", "rewardedId");
            if (string.IsNullOrEmpty(data.max.banner)) data.max.banner = Extract(json, "banner", "Banner", "bannerId");
            if (string.IsNullOrEmpty(data.max.mrec)) data.max.mrec = Extract(json, "mrec", "Mrec", "mrecId");

            // Tracking fallbacks
            if (string.IsNullOrEmpty(data.tracking.platform)) data.tracking.platform = Extract(json, "platform");
            if (string.IsNullOrEmpty(data.tracking.adjustAppToken)) data.tracking.adjustAppToken = Extract(json, "adjustAppToken", "adjustToken", "AdjustToken");
            if (string.IsNullOrEmpty(data.tracking.appsflyerDevKey)) data.tracking.appsflyerDevKey = Extract(json, "appsflyerDevKey", "AppsflyerDevKey", "afDevKey");
            if (string.IsNullOrEmpty(data.tracking.appsflyerAppId)) data.tracking.appsflyerAppId = Extract(json, "appsflyerAppId", "AppsflyerAppId", "afAppId");
            if (string.IsNullOrEmpty(data.tracking.firebaseProjectId)) data.tracking.firebaseProjectId = Extract(json, "firebaseProjectId", "FirebaseProjectId");

            // Legal fallbacks
            if (string.IsNullOrEmpty(data.legal.privacyPolicy)) data.legal.privacyPolicy = Extract(json, "privacyPolicy", "privacy_policy", "privacy");
            if (string.IsNullOrEmpty(data.legal.termsOfUse)) data.legal.termsOfUse = Extract(json, "termsOfUse", "termsOfService", "terms_of_service", "terms");
            if (string.IsNullOrEmpty(data.legal.appAdsTxt)) data.legal.appAdsTxt = Extract(json, "appAdsTxt", "app_ads_txt");
        }

        private static int FindMatchingBrace(string text, int openIdx)
        {
            if (openIdx < 0 || openIdx >= text.Length) return -1;
            int depth = 1;
            for (int i = openIdx + 1; i < text.Length; i++)
            {
                if (text[i] == '{') depth++;
                else if (text[i] == '}')
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
            return -1;
        }

        private static string Extract(string json, params string[] keys)
        {
            if (string.IsNullOrEmpty(json)) return "";
            foreach (var key in keys)
            {
                string searchKey = $"\"{key}\"";
                int idx = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    int colon = json.IndexOf(':', idx + searchKey.Length);
                    if (colon >= 0)
                    {
                        int q1 = json.IndexOf('"', colon + 1);
                        if (q1 >= 0)
                        {
                            int q2 = json.IndexOf('"', q1 + 1);
                            while (q2 > 0 && json[q2 - 1] == '\\')
                            {
                                q2 = json.IndexOf('"', q2 + 1);
                            }
                            if (q2 > q1) return json.Substring(q1 + 1, q2 - q1 - 1).Replace("\\\"", "\"");
                        }
                        else
                        {
                            int start = colon + 1;
                            while (start < json.Length && (json[start] == ' ' || json[start] == '\t' || json[start] == '\r' || json[start] == '\n')) start++;
                            int end = start;
                            while (end < json.Length && json[end] != ',' && json[end] != '}' && json[end] != ']' && json[end] != '\r' && json[end] != '\n') end++;
                            if (end > start) return json.Substring(start, end - start).Trim();
                        }
                    }
                }
            }
            return "";
        }
    }

    [Serializable]
    public class MaxConfig
    {
        public string sdkKey;
        public string admobAppId;
        public string aoa;
        public string appOpen { get => aoa; set => aoa = value; }
        public string inter;
        public string rewarded;
        public string banner;
        public string mrec;
        public string bannerPosition = "BottomCenter";
    }

    [Serializable]
    public class TrackingConfig
    {
        public string platform;
        public string adjustAppToken;
        public string adjustToken { get => adjustAppToken; set => adjustAppToken = value; }
        public string adjustEnvironment = "Production";
        public string appsflyerDevKey;
        public string appsflyerAppId;
        public string firebaseProjectId;
    }

    [Serializable]
    public class LegalConfig
    {
        public string privacyPolicy;
        public string termsOfUse;
        public string termsOfService { get => termsOfUse; set => termsOfUse = value; }
        public string appAdsTxt;
    }
}
