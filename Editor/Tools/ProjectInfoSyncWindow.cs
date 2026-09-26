using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.Core.Services.Dashboard;
using Unity.Dashboard;

namespace Unity.Core.Editor.Tools
{
    /// <summary>
    /// Cửa sổ đồng bộ cấu hình dự án từ Dashboard vào Unity project bằng Mã dự án (Project Code / ID).
    /// API Key được bảo mật và cấu hình cố định trong code (DashboardApiClient.HARDCODED_API_KEY).
    /// </summary>
    public class ProjectInfoSyncWindow : EditorWindow
    {
        private const string PrefKeyBaseUrl = "UnityCore_Dashboard_BaseUrl";
        private const string PrefKeyProjectId = "UnityCore_Dashboard_ProjectId";

        private string _baseUrl = DashboardApiClient.DEFAULT_SERVER_URL;
        private string _projectCode = "";
        private string _targetExportPath = "Assets/Resources/Info.json";

        private ProjectInfoData _fetchedData;
        private string _rawResponseJson = "";
        private string _statusMessage = "";
        private MessageType _statusType = MessageType.Info;
        private bool _isLoading = false;
        private Vector2 _scrollPos;

        [MenuItem("Unity Core/Dashboard/Sync Project Info", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectInfoSyncWindow>("Dashboard Sync");
            window.minSize = new Vector2(540, 520);
            window.Show();
        }

        private void OnEnable()
        {
            _baseUrl = EditorPrefs.GetString(PrefKeyBaseUrl, DashboardApiClient.DEFAULT_SERVER_URL);
            _projectCode = EditorPrefs.GetString(PrefKeyProjectId, "");
        }

        private void SavePrefs()
        {
            EditorPrefs.SetString(PrefKeyBaseUrl, _baseUrl);
            EditorPrefs.SetString(PrefKeyProjectId, _projectCode);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Unity Core - Dashboard Project Info Sync", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Đồng bộ Ad Unit IDs, DevKeys và cấu hình từ máy chủ Dashboard vào Assets/Resources/Info.json bằng Mã dự án.", MessageType.Info);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            // 1. Cấu hình kết nối (Chỉ cần nhập Mã dự án)
            EditorGUILayout.Space(6);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("1. Thông Tin Dự Án (Project Identifier)", EditorStyles.boldLabel);

            _baseUrl = EditorGUILayout.TextField("Dashboard Base URL:", _baseUrl);
            _projectCode = EditorGUILayout.TextField("Mã Dự Án (Project Code):", _projectCode);
            EditorGUILayout.HelpBox("💡 API Key đã được thiết lập bảo mật trong code (DashboardApiClient.HARDCODED_API_KEY).", MessageType.None);
            EditorGUILayout.EndVertical();

            // 2. Nút Fetch
            EditorGUILayout.Space(6);
            EditorGUI.BeginDisabledGroup(_isLoading || string.IsNullOrEmpty(_projectCode));
            if (GUILayout.Button(_isLoading ? "Đang tải dữ liệu từ Dashboard..." : "🔄 Tải Cấu Hình Từ Dashboard", GUILayout.Height(36)))
            {
                SavePrefs();
                FetchProjectInfo();
            }
            EditorGUI.EndDisabledGroup();

            // Hiển thị trạng thái
            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox(_statusMessage, _statusType);
            }

            // 3. Xem trước dữ liệu đã tải
            if (_fetchedData != null)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("2. Xem Trước Dữ Liệu Nhận Được", EditorStyles.boldLabel);

                EditorGUILayout.LabelField("Project Name:", string.IsNullOrEmpty(_fetchedData.Name) ? "(Không có)" : _fetchedData.Name);
                EditorGUILayout.LabelField("Project Code:", _fetchedData.Code);
                EditorGUILayout.LabelField("Platform:", _fetchedData.Platform);
                EditorGUILayout.LabelField("Package Name:", _fetchedData.PackageName);
                EditorGUILayout.LabelField("Apple ID:", _fetchedData.AppleId);
                EditorGUILayout.LabelField("Privacy Policy:", _fetchedData.PrivacyPolicy);
                EditorGUILayout.LabelField("SDK Key (MAX):", _fetchedData.SDKMax);
                EditorGUILayout.LabelField("AdMob App ID:", _fetchedData.AdMobAppId);
                EditorGUILayout.LabelField("App Open (AOA):", _fetchedData.AppOpen);
                EditorGUILayout.LabelField("Banner Ad Unit:", _fetchedData.Banner);
                EditorGUILayout.LabelField("Interstitial Ad Unit:", _fetchedData.Interstitial);
                EditorGUILayout.LabelField("Rewarded Ad Unit:", _fetchedData.Rewarded);
                EditorGUILayout.LabelField("MREC Ad Unit:", _fetchedData.Mrec);
                EditorGUILayout.LabelField("Tracking Platform:", _fetchedData.TrackingPlatform);
                EditorGUILayout.LabelField("Adjust AppToken:", _fetchedData.AdjustToken);
                EditorGUILayout.LabelField("AppsFlyer DevKey:", _fetchedData.AppsflyerDevKey);
                EditorGUILayout.LabelField("Mediation Networks:", $"{_fetchedData.MediationNetworks.Count} networks");

                EditorGUILayout.Space(6);
                _targetExportPath = EditorGUILayout.TextField("Đích Xuất File:", _targetExportPath);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Info.json (Mặc định)")) _targetExportPath = "Assets/Resources/Info.json";
                if (GUILayout.Button("Info_Android.json")) _targetExportPath = "Assets/Resources/Info_Android.json";
                if (GUILayout.Button("Info_iOS.json")) _targetExportPath = "Assets/Resources/Info_iOS.json";
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(6);
                if (GUILayout.Button("💾 Lưu Vào File Cấu Hình (Apply to Project)", GUILayout.Height(38)))
                {
                    SaveToFile();
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
        }

        private async void FetchProjectInfo()
        {
            _isLoading = true;
            _statusMessage = "Đang kết nối tới Dashboard...";
            _statusType = MessageType.Info;
            Repaint();

            try
            {
                var client = new WasdDashboardClient { BaseUrl = _baseUrl };
                _fetchedData = await client.FetchProjectInfoAsync(_projectCode, DashboardApiClient.HARDCODED_API_KEY);
                _rawResponseJson = _fetchedData.RawJson;
                _statusMessage = $"Đã tải cấu hình thành công cho dự án '{_fetchedData.Name}' (Mã: {_projectCode})!";
                _statusType = MessageType.Info;
            }
            catch (Exception ex)
            {
                _statusMessage = $"Lỗi kết nối Dashboard: {ex.Message}";
                _statusType = MessageType.Error;
                _fetchedData = null;
            }
            finally
            {
                _isLoading = false;
                Repaint();
            }
        }

        private void SaveToFile()
        {
            if (_fetchedData == null) return;

            try
            {
                string fullPath = Path.Combine(Application.dataPath, "..", _targetExportPath);
                string dir = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string jsonContent = _fetchedData.ToFormattedJson();
                File.WriteAllText(fullPath, jsonContent);
                AssetDatabase.Refresh();

                _statusMessage = $"Đã lưu cấu hình thành công vào: {_targetExportPath}";
                _statusType = MessageType.Info;
                EditorUtility.DisplayDialog("Thành công", $"Đã cập nhật cấu hình dự án từ Dashboard vào: {_targetExportPath}", "OK");
            }
            catch (Exception ex)
            {
                _statusMessage = $"Lỗi ghi file: {ex.Message}";
                _statusType = MessageType.Error;
                EditorUtility.DisplayDialog("Lỗi", $"Lỗi ghi file: {ex.Message}", "OK");
            }
        }
    }
}
