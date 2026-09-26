using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Unity.Core.Services.Dashboard
{
    /// <summary>
    /// Model đại diện cho thông tin cấu hình dự án nhận được từ WASD Dashboard API:
    /// Lưu trữ đầy đủ 100% các cấu hình Ads (MAX, AdMob, AOA, MREC), Tracking (Adjust, AppsFlyer), Legal và Mediation Networks.
    /// </summary>
    [Serializable]
    public class ProjectInfoData
    {
        // Thông tin dự án cơ bản
        public string ProjectId = "";
        public string Code = "";
        public string Name = "";
        public string Platform = "";
        public string PackageName = "";
        public string AppleId = "";
        public string Version = "1.0.0";

        // Cấu hình Quảng cáo (AppLovin MAX / AdMob)
        public string SDKMax = "";
        public string AdMobAppId = "";
        public string AppOpen = "";       // maps từ 'aoa', 'appOpen', 'AppOpen'
        public string Banner = "";
        public string Interstitial = "";  // maps từ 'inter', 'interstitial', 'Interstitial'
        public string Rewarded = "";      // maps từ 'rewarded', 'Rewarded'
        public string Mrec = "";          // maps từ 'mrec', 'Mrec'
        public string BannerPosition = "BottomCenter";

        // Cấu hình Tracking & Attribution
        public string TrackingPlatform = "";
        public string AdjustToken = "";   // maps từ 'adjustAppToken', 'adjustToken', 'AdjustToken'
        public string AdjustEnvironment = "Production";
        public string AppsflyerDevKey = "";
        public string AppsflyerAppId = "";
        public string FirebaseProjectId = "";

        // Thông tin Pháp lý (Legal)
        public string PrivacyPolicy = "";
        public string TermsOfUse = "";
        public string AppAdsTxt = "";

        // Mediation Networks
        public List<MediationNetworkInfo> MediationNetworks = new List<MediationNetworkInfo>();

        // Payload gốc
        public string RawJson = "{}";

        public static ProjectInfoData FromJson(string json)
        {
            var data = new ProjectInfoData();
            if (string.IsNullOrEmpty(json)) return data;

            data.RawJson = json;

            // Đọc thông tin cơ bản
            data.ProjectId = ExtractValue(json, "id", "_id", "projectId", "ProjectId");
            data.Code = ExtractValue(json, "code", "projectCode", "Code");
            data.Name = ExtractValue(json, "name", "Name", "projectName", "title");
            data.Platform = ExtractValue(json, "platform", "Platform");
            data.PackageName = ExtractValue(json, "packageName", "PackageName", "bundleId", "appId");
            data.AppleId = ExtractValue(json, "appleId", "apple_id", "AppleId");
            data.Version = ExtractValue(json, "version", "Version", "appVersion");
            if (string.IsNullOrEmpty(data.Version)) data.Version = "1.0.0";

            // Cấu hình Ads (MAX / AdMob)
            data.SDKMax = ExtractValue(json, "sdkKey", "SDKMax", "sdkMax", "applovinSdkKey");
            data.AdMobAppId = ExtractValue(json, "admobAppId", "admob_app_id", "admobAppID");
            data.AppOpen = ExtractValue(json, "aoa", "appOpen", "AppOpen", "appOpenId");
            data.Banner = ExtractValue(json, "banner", "Banner", "bannerId");
            data.Interstitial = ExtractValue(json, "inter", "interstitial", "Interstitial", "interstitialId");
            data.Rewarded = ExtractValue(json, "rewarded", "Rewarded", "rewardedId");
            data.Mrec = ExtractValue(json, "mrec", "Mrec", "mrecId");
            data.BannerPosition = ExtractValue(json, "bannerPosition", "BannerPosition");
            if (string.IsNullOrEmpty(data.BannerPosition)) data.BannerPosition = "BottomCenter";

            // Cấu hình Tracking
            data.TrackingPlatform = ExtractValue(json, "platform", "trackingPlatform");
            data.AdjustToken = ExtractValue(json, "adjustAppToken", "adjustToken", "AdjustToken", "adjust_token");
            data.AdjustEnvironment = ExtractValue(json, "adjustEnvironment", "AdjustEnvironment", "environment");
            if (string.IsNullOrEmpty(data.AdjustEnvironment)) data.AdjustEnvironment = "Production";

            data.AppsflyerDevKey = ExtractValue(json, "appsflyerDevKey", "AppsflyerDevKey", "afDevKey");
            data.AppsflyerAppId = ExtractValue(json, "appsflyerAppId", "AppsflyerAppId", "afAppId");
            data.FirebaseProjectId = ExtractValue(json, "firebaseProjectId", "FirebaseProjectId", "firebaseProject");

            // Cấu hình Legal
            data.PrivacyPolicy = ExtractValue(json, "privacyPolicy", "privacy_policy", "PrivacyPolicy");
            data.TermsOfUse = ExtractValue(json, "termsOfUse", "termsOfService", "terms_of_use", "TermsOfUse");
            data.AppAdsTxt = ExtractValue(json, "appAdsTxt", "app_ads_txt", "AppAdsTxt");

            // Mediation Networks
            ExtractMediationNetworks(json, data.MediationNetworks);

            return data;
        }

        public string GetString(string key, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(RawJson)) return defaultValue;
            string val = ExtractValue(RawJson, key);
            return string.IsNullOrEmpty(val) ? defaultValue : val;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            string val = GetString(key);
            return int.TryParse(val, out int result) ? result : defaultValue;
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            string val = GetString(key);
            return float.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result) ? result : defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            string val = GetString(key);
            if (string.IsNullOrEmpty(val)) return defaultValue;
            if (val.Equals("true", StringComparison.OrdinalIgnoreCase) || val == "1") return true;
            if (val.Equals("false", StringComparison.OrdinalIgnoreCase) || val == "0") return false;
            return defaultValue;
        }

        /// <summary>
        /// Xuất file JSON đầy đủ 100% dữ liệu có format đẹp mắt cho Assets/Resources/Info.json.
        /// </summary>
        public string ToFormattedJson()
        {
            // Nếu có RawJson chứa block "data", trích xuất và format block "data" đó
            if (!string.IsNullOrEmpty(RawJson) && RawJson.Contains("\"data\""))
            {
                int dataIdx = RawJson.IndexOf("\"data\"", StringComparison.Ordinal);
                if (dataIdx >= 0)
                {
                    int openBrace = RawJson.IndexOf('{', dataIdx);
                    if (openBrace >= 0)
                    {
                        int closeBrace = FindMatchingBrace(RawJson, openBrace);
                        if (closeBrace > openBrace)
                        {
                            string dataBlock = RawJson.Substring(openBrace, closeBrace - openBrace + 1);
                            return FormatJson(dataBlock);
                        }
                    }
                }
            }

            // Fallback: Tự serialize toàn bộ cấu trúc đầy đủ
            return BuildFullFormattedJson();
        }

        private string BuildFullFormattedJson()
        {
            var sb = new StringBuilder("{\n");
            sb.AppendLine($"  \"name\": \"{Escape(Name)}\",");
            sb.AppendLine($"  \"code\": \"{Escape(Code)}\",");
            sb.AppendLine($"  \"platform\": \"{Escape(Platform)}\",");
            sb.AppendLine($"  \"packageName\": \"{Escape(PackageName)}\",");
            sb.AppendLine($"  \"appleId\": \"{Escape(AppleId)}\",");
            
            // Legal
            sb.AppendLine("  \"legal\": {");
            sb.AppendLine($"    \"privacyPolicy\": \"{Escape(PrivacyPolicy)}\",");
            sb.AppendLine($"    \"termsOfUse\": \"{Escape(TermsOfUse)}\",");
            sb.AppendLine($"    \"appAdsTxt\": \"{Escape(AppAdsTxt)}\"");
            sb.AppendLine("  },");

            // Max
            sb.AppendLine("  \"max\": {");
            sb.AppendLine($"    \"sdkKey\": \"{Escape(SDKMax)}\",");
            sb.AppendLine($"    \"admobAppId\": \"{Escape(AdMobAppId)}\",");
            sb.AppendLine($"    \"aoa\": \"{Escape(AppOpen)}\",");
            sb.AppendLine($"    \"inter\": \"{Escape(Interstitial)}\",");
            sb.AppendLine($"    \"banner\": \"{Escape(Banner)}\",");
            sb.AppendLine($"    \"rewarded\": \"{Escape(Rewarded)}\",");
            sb.AppendLine($"    \"mrec\": \"{Escape(Mrec)}\"");
            sb.AppendLine("  },");

            // Tracking
            sb.AppendLine("  \"tracking\": {");
            sb.AppendLine($"    \"platform\": \"{Escape(TrackingPlatform)}\",");
            sb.AppendLine($"    \"adjustAppToken\": \"{Escape(AdjustToken)}\",");
            sb.AppendLine($"    \"appsflyerDevKey\": \"{Escape(AppsflyerDevKey)}\",");
            sb.AppendLine($"    \"appsflyerAppId\": \"{Escape(AppsflyerAppId)}\",");
            sb.AppendLine($"    \"firebaseProjectId\": \"{Escape(FirebaseProjectId)}\"");
            sb.AppendLine("  },");

            // Mediation Networks
            sb.AppendLine("  \"mediationNetworks\": [");
            for (int i = 0; i < MediationNetworks.Count; i++)
            {
                var net = MediationNetworks[i];
                sb.Append($"    {{ \"name\": \"{Escape(net.name)}\", \"id\": \"{Escape(net.id)}\" }}");
                if (i < MediationNetworks.Count - 1) sb.Append(",");
                sb.AppendLine();
            }
            sb.AppendLine("  ]");

            sb.Append("}");
            return sb.ToString();
        }

        public static string FormatJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return "{}";

            var sb = new StringBuilder();
            bool inQuotes = false;
            bool isEscaped = false;
            int indent = 0;

            for (int i = 0; i < json.Length; i++)
            {
                char ch = json[i];

                if (isEscaped)
                {
                    sb.Append(ch);
                    isEscaped = false;
                    continue;
                }

                if (ch == '\\')
                {
                    sb.Append(ch);
                    isEscaped = true;
                    continue;
                }

                if (ch == '"')
                {
                    inQuotes = !inQuotes;
                    sb.Append(ch);
                    continue;
                }

                if (inQuotes)
                {
                    sb.Append(ch);
                    continue;
                }

                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        sb.AppendLine();
                        indent += 2;
                        sb.Append(new string(' ', indent));
                        break;
                    case '}':
                    case ']':
                        sb.AppendLine();
                        indent = Math.Max(0, indent - 2);
                        sb.Append(new string(' ', indent));
                        sb.Append(ch);
                        break;
                    case ',':
                        sb.Append(ch);
                        sb.AppendLine();
                        sb.Append(new string(' ', indent));
                        break;
                    case ':':
                        sb.Append(": ");
                        break;
                    default:
                        if (!char.IsWhiteSpace(ch))
                        {
                            sb.Append(ch);
                        }
                        break;
                }
            }

            return sb.ToString().Trim();
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

        private static string Escape(string val)
        {
            if (string.IsNullOrEmpty(val)) return "";
            return val.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");
        }

        private static void ExtractMediationNetworks(string json, List<MediationNetworkInfo> list)
        {
            list.Clear();
            int netIdx = json.IndexOf("\"mediationNetworks\"", StringComparison.OrdinalIgnoreCase);
            if (netIdx < 0) return;

            int openBracket = json.IndexOf('[', netIdx);
            int closeBracket = json.IndexOf(']', openBracket >= 0 ? openBracket : 0);
            if (openBracket < 0 || closeBracket <= openBracket) return;

            string block = json.Substring(openBracket, closeBracket - openBracket + 1);
            int cur = 0;
            while (cur < block.Length)
            {
                int itemOpen = block.IndexOf('{', cur);
                if (itemOpen < 0) break;
                int itemClose = block.IndexOf('}', itemOpen);
                if (itemClose < 0) break;

                string itemStr = block.Substring(itemOpen, itemClose - itemOpen + 1);
                string name = ExtractValue(itemStr, "name");
                string id = ExtractValue(itemStr, "id");
                if (!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(id))
                {
                    list.Add(new MediationNetworkInfo { name = name, id = id });
                }
                cur = itemClose + 1;
            }
        }

        private static string ExtractValue(string json, params string[] keys)
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
                            if (q2 > q1)
                            {
                                return json.Substring(q1 + 1, q2 - q1 - 1).Replace("\\\"", "\"");
                            }
                        }
                        else
                        {
                            int start = colon + 1;
                            while (start < json.Length && (json[start] == ' ' || json[start] == '\t' || json[start] == '\r' || json[start] == '\n')) start++;
                            int end = start;
                            while (end < json.Length && json[end] != ',' && json[end] != '}' && json[end] != ']' && json[end] != '\r' && json[end] != '\n') end++;
                            if (end > start)
                            {
                                return json.Substring(start, end - start).Trim();
                            }
                        }
                    }
                }
            }
            return "";
        }
    }

    [Serializable]
    public class MediationNetworkInfo
    {
        public string name = "";
        public string id = "";
    }
}
