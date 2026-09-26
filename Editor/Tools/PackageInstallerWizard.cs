using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Unity.Core.Editor.Tools
{
    public class PackageInstallerWizard : EditorWindow
    {
        private class WasdPackageInfo
        {
            public string PackageId;
            public string DisplayName;
            public string Description;
            public string RepoName;
            public string CustomGitUrl;
            public bool IsRequired;
            public bool IsSelected;
            public bool IsInstalled;
        }

        private int _selectedTab = 0;
        private readonly string[] _tabTitles = new string[] { "📦 Packages", "🔌 Plugins" };

        // Danh sách WASD Packages
        private List<WasdPackageInfo> _packages;

        // Danh sách Plugins / 3rd-Party SDKs
        private List<WasdPackageInfo> _plugins;

        // Cấu hình Git chung
        private string _gitOrgUrl = "https://github.com/thoxuong92/";
        private Vector2 _scrollPos;
        private AddRequest _addRequest;
        private RemoveRequest _removeRequest;
        private Queue<string> _updateQueue = new Queue<string>();
        private string _currentProcessingName = "";

        // Cài đặt Plugin Custom
        private string _customPluginName = "";
        private string _customPluginGitUrl = "https://github.com/wasdgamestudio/";

        [MenuItem("Unity Core/Package Manager Hub", false, 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<PackageInstallerWizard>("Unity Core Package Hub");
            window.minSize = new Vector2(650, 520);
            window.Show();
        }

        private void OnEnable()
        {
            InitPackages();
            InitPlugins();
            RefreshInstalledStatus();
        }

        private void OnDisable()
        {
            EditorApplication.update -= ProgressCallback;
        }

        private void InitPackages()
        {
            _packages = new List<WasdPackageInfo>
            {
                new WasdPackageInfo
                {
                    PackageId = "com.unity.core",
                    DisplayName = "Unity Core Framework",
                    Description = "Service Locator, Event Bus, FSM, Object Pooling & Conditional Logger.",
                    RepoName = "com.unity.core.git",
                    IsRequired = true,
                    IsSelected = true
                },
                new WasdPackageInfo
                {
                    PackageId = "com.wasd.ui",
                    DisplayName = "WASD UI Navigation",
                    Description = "Screen & Popup Stack Management, Safe Area Fitter for iOS Notch & Android punch-hole.",
                    RepoName = "com.wasd.ui.git",
                    IsSelected = true
                },
                new WasdPackageInfo
                {
                    PackageId = "com.wasd.audio",
                    DisplayName = "WASD Audio Manager",
                    Description = "BGM & SFX multi-channel audio player with automatic pooling & mute handlers.",
                    RepoName = "com.wasd.audio.git",
                    IsSelected = true
                },
                new WasdPackageInfo
                {
                    PackageId = "com.wasd.save",
                    DisplayName = "WASD Encrypted Save",
                    Description = "Secure AES-encrypted JSON data persistence & automatic save state wrapper.",
                    RepoName = "com.wasd.save.git",
                    IsSelected = true
                },
                new WasdPackageInfo
                {
                    PackageId = "com.wasd.ads",
                    DisplayName = "WASD Ads Wrapper",
                    Description = "Unified IAdsService with pluggable adapters (AdMob, AppLovin MAX, Unity Ads).",
                    RepoName = "com.wasd.ads.git",
                    IsSelected = false
                },
                new WasdPackageInfo
                {
                    PackageId = "com.wasd.analytics",
                    DisplayName = "WASD Analytics Hub",
                    Description = "Unified event logging for Firebase, GameAnalytics, AppsFlyer.",
                    RepoName = "com.wasd.analytics.git",
                    IsSelected = false
                },
                new WasdPackageInfo
                {
                    PackageId = "com.unity.firebase",
                    DisplayName = "Unity Firebase Service",
                    Description = "Firebase Analytics, Remote Config, Ad Revenue Attribution & Editor Sync Tooling.",
                    RepoName = "com.unity.firebase.git",
                    IsSelected = false
                },
                new WasdPackageInfo
                {
                    PackageId = "com.wasd.applovin",
                    DisplayName = "WASD AppLovin MAX Service",
                    Description = "AppLovin MAX Mediation Ads Service Adapter with Mediation Network Setup.",
                    RepoName = "com.wasd.applovin.git",
                    IsSelected = false
                },
                new WasdPackageInfo
                {
                    PackageId = "com.wasd.appsflyer",
                    DisplayName = "WASD AppsFlyer Service",
                    Description = "AppsFlyer Attribution, In-App Analytics & Impression-Level Ad Revenue Tracking.",
                    RepoName = "com.wasd.appsflyer.git",
                    IsSelected = false
                },
                new WasdPackageInfo
                {
                    PackageId = "com.wasd.input",
                    DisplayName = "WASD Mobile Touch Input",
                    Description = "Virtual Joystick (Fixed/Floating/Dynamic), Swipe Look Touchpad, Action Buttons & Gestures.",
                    RepoName = "com.wasd.input.git",
                    IsSelected = true
                },
                new WasdPackageInfo
                {
                    PackageId = "com.wasd.adjust",
                    DisplayName = "WASD Adjust Service",
                    Description = "Adjust Attribution, Deep Linking, SKAdNetwork & Impression-Level Ad Revenue Tracking.",
                    RepoName = "com.wasd.adjust.git",
                    IsSelected = false
                }
            };
        }

        private void InitPlugins()
        {
            _plugins = new List<WasdPackageInfo>
            {
                new WasdPackageInfo
                {
                    PackageId = "com.applovin.mediation.ads",
                    DisplayName = "AppLovin MAX SDK",
                    Description = "Bộ SDK AppLovin MAX Mediation hỗ trợ quản lý quảng cáo Interstitial, Rewarded, Banner & App Open.",
                    CustomGitUrl = "https://github.com/wasdgamestudio/applovin-max.git",
                    IsSelected = false
                }
            };
        }

        private void RefreshInstalledStatus()
        {
            string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (!File.Exists(manifestPath)) return;

            string content = File.ReadAllText(manifestPath);
            if (_packages != null)
            {
                foreach (var pkg in _packages)
                {
                    pkg.IsInstalled = content.Contains(pkg.PackageId) || 
                                     (!string.IsNullOrEmpty(pkg.RepoName) && content.Contains(pkg.RepoName));
                }
            }

            if (_plugins != null)
            {
                foreach (var plugin in _plugins)
                {
                    plugin.IsInstalled = (!string.IsNullOrEmpty(plugin.PackageId) && content.Contains(plugin.PackageId)) ||
                                         (!string.IsNullOrEmpty(plugin.CustomGitUrl) && content.Contains(plugin.CustomGitUrl)) ||
                                         content.Contains("applovin-max");
                }
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Unity Core - Modular Package & Plugin Hub", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Quản lý, cài đặt và cập nhật các module package / plugins chuẩn UPM trực tiếp từ kho Git.", MessageType.Info);

            // Hiển thị trạng thái đang cập nhật nếu có
            if (!string.IsNullOrEmpty(_currentProcessingName) || (_updateQueue != null && _updateQueue.Count > 0))
            {
                int count = _updateQueue != null ? _updateQueue.Count : 0;
                EditorGUILayout.HelpBox($"⏳ Đang cập nhật: {_currentProcessingName} (Còn {count} package trong hàng đợi)...", MessageType.Warning);
            }

            EditorGUILayout.Space(6);
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabTitles, GUILayout.Height(30));

            EditorGUILayout.Space(8);
            if (_selectedTab == 0)
            {
                DrawPackagesTab();
            }
            else
            {
                DrawPluginsTab();
            }
        }

        private void DrawPackagesTab()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _gitOrgUrl = EditorGUILayout.TextField("GitHub Base URL:", _gitOrgUrl);

            EditorGUILayout.Space(4);
            if (GUILayout.Button("🌐 Đồng Bộ Cấu Hình Dự Án Từ Dashboard...", GUILayout.Height(26)))
            {
                ProjectInfoSyncWindow.ShowWindow();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);
            GUILayout.Label("Available Modules:", EditorStyles.boldLabel);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(260));
            if (_packages != null)
            {
                foreach (var pkg in _packages)
                {
                    DrawPackageItem(pkg, false);
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginDisabledGroup(_addRequest != null || (_updateQueue != null && _updateQueue.Count > 0));
            if (GUILayout.Button("Install Selected", GUILayout.Height(35)))
            {
                InstallSelectedPackages(_packages);
            }

            GUI.backgroundColor = new Color(0.35f, 0.75f, 1f);
            if (GUILayout.Button("🔄 Cập Nhật Tất Cả Từ Git (Update All)", GUILayout.Height(35)))
            {
                UpdateAllInstalledPackages(_packages);
            }
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("Refresh Status", GUILayout.Height(35), GUILayout.Width(110)))
            {
                RefreshInstalledStatus();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPluginsTab()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Thư Viện Plugins & SDKs Bên Thứ 3", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Cài đặt nhanh các SDK ngoài (AppLovin MAX, Tracking SDKs...) kéo trực tiếp từ Git của Studio.", MessageType.None);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);
            GUILayout.Label("Studio Plugins List:", EditorStyles.boldLabel);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(200));
            if (_plugins != null)
            {
                foreach (var plugin in _plugins)
                {
                    DrawPackageItem(plugin, true);
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8);
            // Form thêm/cài đặt plugin tùy ý từ Git URL
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("➕ Thêm / Cài Đặt Plugin Mới Từ Git URL", EditorStyles.boldLabel);
            _customPluginName = EditorGUILayout.TextField("Tên Plugin (Tùy chọn):", _customPluginName);
            _customPluginGitUrl = EditorGUILayout.TextField("Git URL:", _customPluginGitUrl);

            EditorGUILayout.Space(4);
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_customPluginGitUrl) || _customPluginGitUrl == "https://github.com/wasdgamestudio/" || _addRequest != null);
            if (GUILayout.Button("⚡ Cài Đặt Plugin Qua Git URL Này", GUILayout.Height(30)))
            {
                InstallGitUrlDirectly(_customPluginGitUrl);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginDisabledGroup(_addRequest != null || (_updateQueue != null && _updateQueue.Count > 0));
            if (GUILayout.Button("Install Selected Plugins", GUILayout.Height(35)))
            {
                InstallSelectedPackages(_plugins);
            }

            GUI.backgroundColor = new Color(0.35f, 0.75f, 1f);
            if (GUILayout.Button("🔄 Cập Nhật Tất Cả Plugins (Update All)", GUILayout.Height(35)))
            {
                UpdateAllInstalledPackages(_plugins);
            }
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("Refresh Status", GUILayout.Height(35), GUILayout.Width(110)))
            {
                RefreshInstalledStatus();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPackageItem(WasdPackageInfo pkg, bool isPlugin)
        {
            if (pkg == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !pkg.IsRequired;
            pkg.IsSelected = EditorGUILayout.Toggle(pkg.IsSelected, GUILayout.Width(20));
            GUI.enabled = true;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(pkg.DisplayName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(pkg.Description, EditorStyles.wordWrappedMiniLabel);
            
            string sourceUrl = GetGitUrl(pkg);
            EditorGUILayout.LabelField($"Source: {sourceUrl}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            if (pkg.IsInstalled)
            {
                GUILayout.Label("✓ Installed", EditorStyles.boldLabel, GUILayout.Width(75));

                // Nút Cập nhật riêng từng package từ Git
                GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
                if (GUILayout.Button("🔄 Reload", GUILayout.Width(65)))
                {
                    UpdatePackage(pkg);
                }
                GUI.backgroundColor = Color.white;

                if (!pkg.IsRequired && GUILayout.Button("Remove", GUILayout.Width(65)))
                {
                    UninstallPackage(pkg);
                }
            }
            else
            {
                if (GUILayout.Button("Install", GUILayout.Width(75)))
                {
                    InstallPackage(pkg);
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }

        private string GetGitUrl(WasdPackageInfo pkg)
        {
            if (pkg == null) return "";
            if (!string.IsNullOrEmpty(pkg.CustomGitUrl))
            {
                return pkg.CustomGitUrl.Trim();
            }
            return $"{_gitOrgUrl.TrimEnd('/')}/{pkg.RepoName}".Trim();
        }

        private void InstallPackage(WasdPackageInfo pkg)
        {
            if (pkg == null) return;
            string gitUrl = GetGitUrl(pkg);
            InstallGitUrlDirectly(gitUrl, pkg.DisplayName);
        }

        private void UpdatePackage(WasdPackageInfo pkg)
        {
            if (pkg == null) return;
            string gitUrl = GetGitUrl(pkg);
            Debug.Log($"[Unity Core Package Hub] 🔄 Đang tải phiên bản Git mới nhất cho: {pkg.DisplayName} ({gitUrl})...");
            InstallGitUrlDirectly(gitUrl, pkg.DisplayName);
        }

        private void UpdateAllInstalledPackages(List<WasdPackageInfo> list)
        {
            if (list == null) return;

            if (_updateQueue == null) _updateQueue = new Queue<string>();
            _updateQueue.Clear();

            foreach (var pkg in list)
            {
                if (pkg != null && pkg.IsInstalled)
                {
                    _updateQueue.Enqueue(GetGitUrl(pkg));
                }
            }

            if (_updateQueue.Count > 0)
            {
                Debug.Log($"[Unity Core Package Hub] 🔄 Bắt đầu cập nhật {_updateQueue.Count} packages đã cài đặt từ Git...");
                ProcessNextInQueue();
            }
            else
            {
                EditorUtility.DisplayDialog("Thông báo", "Chưa có package nào được cài đặt để cập nhật.", "OK");
            }
        }

        private void ProcessNextInQueue()
        {
            if (_updateQueue == null || _updateQueue.Count == 0)
            {
                _currentProcessingName = "";
                RefreshInstalledStatus();
                Repaint();
                Debug.Log("[Unity Core Package Hub] ✅ Đã hoàn tất cập nhật tất cả package lên bản mới nhất từ Git!");
                return;
            }

            string nextUrl = _updateQueue.Dequeue();
            _currentProcessingName = nextUrl;
            Debug.Log($"[Unity Core Package Hub] Đang cập nhật: {nextUrl}...");
            _addRequest = Client.Add(nextUrl);
            
            EditorApplication.update -= ProgressCallback;
            EditorApplication.update += ProgressCallback;
            Repaint();
        }

        private void InstallGitUrlDirectly(string gitUrl, string packageIdForLog = null)
        {
            if (string.IsNullOrEmpty(gitUrl)) return;
            string logTarget = string.IsNullOrEmpty(packageIdForLog) ? gitUrl : packageIdForLog;
            _currentProcessingName = logTarget;
            Debug.Log($"[Unity Core Package Hub] Installing/Updating {logTarget} from {gitUrl}");
            _addRequest = Client.Add(gitUrl.Trim());

            EditorApplication.update -= ProgressCallback;
            EditorApplication.update += ProgressCallback;
            Repaint();
        }

        private void UninstallPackage(WasdPackageInfo pkg)
        {
            if (pkg == null) return;
            string idToRemove = !string.IsNullOrEmpty(pkg.PackageId) ? pkg.PackageId : pkg.CustomGitUrl;
            Debug.Log($"[Unity Core Package Hub] Removing {idToRemove}");
            _removeRequest = Client.Remove(idToRemove);

            EditorApplication.update -= ProgressCallback;
            EditorApplication.update += ProgressCallback;
        }

        private void InstallSelectedPackages(List<WasdPackageInfo> list)
        {
            if (list == null) return;
            foreach (var pkg in list)
            {
                if (pkg != null && pkg.IsSelected && !pkg.IsInstalled)
                {
                    InstallPackage(pkg);
                }
            }
        }

        private void ProgressCallback()
        {
            try
            {
                if (_addRequest != null && _addRequest.IsCompleted)
                {
                    if (_addRequest.Status == StatusCode.Success)
                    {
                        string pkgId = _addRequest.Result != null ? _addRequest.Result.packageId : _currentProcessingName;
                        Debug.Log($"[Unity Core Package Hub] ✅ Successfully updated/installed: {pkgId}");
                    }
                    else if (_addRequest.Status >= StatusCode.Failure)
                    {
                        string errorMsg = (_addRequest.Error != null && !string.IsNullOrEmpty(_addRequest.Error.message)) 
                            ? _addRequest.Error.message 
                            : "Unknown error";
                        Debug.LogError($"[Unity Core Package Hub] ❌ Update/Install failed: {errorMsg}");
                    }

                    _addRequest = null;

                    // Nếu còn trong hàng đợi Update All, tiếp tục chạy package kế tiếp
                    if (_updateQueue != null && _updateQueue.Count > 0)
                    {
                        ProcessNextInQueue();
                        return;
                    }
                    else
                    {
                        _currentProcessingName = "";
                        RefreshInstalledStatus();
                        Repaint();
                    }
                }

                if (_removeRequest != null && _removeRequest.IsCompleted)
                {
                    if (_removeRequest.Status == StatusCode.Success)
                    {
                        string removedName = !string.IsNullOrEmpty(_removeRequest.PackageIdOrName) 
                            ? _removeRequest.PackageIdOrName 
                            : "Package";
                        Debug.Log($"[Unity Core Package Hub] Successfully removed: {removedName}");
                    }
                    else if (_removeRequest.Status >= StatusCode.Failure)
                    {
                        string errorMsg = (_removeRequest.Error != null && !string.IsNullOrEmpty(_removeRequest.Error.message)) 
                            ? _removeRequest.Error.message 
                            : "Unknown error";
                        Debug.LogError($"[Unity Core Package Hub] Remove failed: {errorMsg}");
                    }

                    _removeRequest = null;
                    RefreshInstalledStatus();
                    Repaint();
                }

                if (_addRequest == null && _removeRequest == null && (_updateQueue == null || _updateQueue.Count == 0))
                {
                    EditorApplication.update -= ProgressCallback;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Unity Core Package Hub] Callback notice: {ex.Message}");
                _addRequest = null;
                _removeRequest = null;
                _currentProcessingName = "";
                EditorApplication.update -= ProgressCallback;
            }
        }
    }
}
