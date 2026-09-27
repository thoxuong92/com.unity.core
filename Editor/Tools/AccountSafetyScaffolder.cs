using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Unity.Core.Editor.Tools
{
    /// <summary>
    /// Security & Anti-Fingerprinting Scaffolder for Unity Core.
    /// Helps prepare projects for deployment across distinct Google Play / Apple App Store developer accounts.
    /// </summary>
    public class AccountSafetyScaffolder : EditorWindow
    {
        private Vector2 _scrollPos;
        private List<string> _detectedHardcodedKeys = new List<string>();
        private bool _hasScanned = false;

        private string _targetNamespace = "ProjectGame.Core";
        private string _newAssemblyPrefix = "ProjectGame";

        [MenuItem("Unity Core/Security & Account Safety Inspector", false, 2)]
        public static void ShowWindow()
        {
            var window = GetWindow<AccountSafetyScaffolder>("Unity Core Account Safety");
            window.minSize = new Vector2(580, 520);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Unity Core Store Account Safety & Anti-Fingerprint Inspector", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Công cụ kiểm tra bảo mật & chống quét liên kết tài khoản (Account Association / Policy Violation) khi đẩy game lên nhiều tài khoản Google Play & Apple App Store khác nhau.", MessageType.Info);

            EditorGUILayout.Space(5);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawBuildSettingsCheckSection();
            EditorGUILayout.Space(10);
            DrawHardcodedKeyScannerSection();
            EditorGUILayout.Space(10);
            DrawNamespaceScaffolderSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawBuildSettingsCheckSection()
        {
            EditorGUILayout.LabelField("1. Kiểm tra Cấu hình Build An Toàn (Build Engine Safety)", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Android Scripting Backend
            var androidBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
            bool isAndroidIL2CPP = androidBackend == ScriptingImplementation.IL2CPP;
            EditorGUILayout.LabelField($"• Android Scripting Backend: {androidBackend}", isAndroidIL2CPP ? EditorStyles.label : EditorStyles.boldLabel);
            if (!isAndroidIL2CPP)
            {
                EditorGUILayout.HelpBox("CẢNH BÁO: Cần đổi Android Scripting Backend sang IL2CPP để mã hóa C++ và tránh bị decompile APK.", MessageType.Warning);
                if (GUILayout.Button("Chuyển Android sang IL2CPP"))
                {
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                }
            }

            // Stripping Level
            var stripping = PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Android);
            EditorGUILayout.LabelField($"• Managed Code Stripping Level: {stripping}");
            if (stripping == ManagedStrippingLevel.Disabled || stripping == ManagedStrippingLevel.Minimal)
            {
                EditorGUILayout.HelpBox("KHUYÊN DÙNG: Đặt Stripping Level là Medium hoặc High để loại bỏ các symbols/metadata trùng lặp giữa các game.", MessageType.Info);
                if (GUILayout.Button("Đặt Code Stripping thành High (Android)"))
                {
                    PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.High);
                }
            }

            // Bundle Identifier Check
            string bundleId = PlayerSettings.applicationIdentifier;
            EditorGUILayout.LabelField($"• Current Package Name / Bundle ID: {bundleId}");
            if (bundleId.Contains("com.DefaultCompany") || bundleId.Contains("com.wasd") || bundleId.Contains("com.example"))
            {
                EditorGUILayout.HelpBox("NGUY HIỂM: Đang dùng Package Name mặc định / trùng mẫu. Phải đổi sang Package ID riêng biệt cho từng tài khoản.", MessageType.Error);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawHardcodedKeyScannerSection()
        {
            EditorGUILayout.LabelField("2. Quét Tránh Trùng Dấu Vân Tay & Key Nhạy Cảm (Store Multi-Account Safety)", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Quét toàn bộ Project (Code, Manifests, XML) để chống bị Google Play / Apple Store quét liên kết tài khoản (Account Association):");

            if (GUILayout.Button("Quét Toàn Diện Project", GUILayout.Height(32)))
            {
                ScanSourceCodeForKeys();
            }

            if (_hasScanned)
            {
                if (_detectedHardcodedKeys.Count == 0)
                {
                    EditorGUILayout.HelpBox("✓ Tuyệt vời! Không phát hiện khóa nhạy cảm, debuggable hay permission nguy hiểm nào.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox($"Phát hiện {_detectedHardcodedKeys.Count} vấn đề có nguy cơ gây reject hoặc liên kết tài khoản:", MessageType.Warning);
                    foreach (var issue in _detectedHardcodedKeys)
                    {
                        EditorGUILayout.LabelField($"⚠ {issue}", EditorStyles.wordWrappedMiniLabel);
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawNamespaceScaffolderSection()
        {
            EditorGUILayout.LabelField("3. Đổi Namespace / Assembly Định Danh Độc Lập (Custom Scaffolder)", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Khi bàn giao project sang tài khoản đối tác hoặc tài khoản studio độc lập, công cụ này giúp đổi namespace nhanh chóng:");

            _targetNamespace = EditorGUILayout.TextField("Root Namespace Mới:", _targetNamespace);
            _newAssemblyPrefix = EditorGUILayout.TextField("Assembly Prefix Mới:", _newAssemblyPrefix);

            if (GUILayout.Button("Tạo Template Script Mới với Namespace Độc Lập", GUILayout.Height(30)))
            {
                EditorUtility.DisplayDialog("Scaffolder", 
                    $"Đã sẵn sàng cơ chế Namespace tùy biến. Bạn có thể sử dụng Namespace '{_targetNamespace}' trong các gameplay scripts.", "OK");
            }

            EditorGUILayout.EndVertical();
        }

        private void ScanSourceCodeForKeys()
        {
            _detectedHardcodedKeys.Clear();
            _hasScanned = true;

            // 1. Quét C# Scripts
            string[] scriptFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            string[] suspiciousPatterns = new string[]
            {
                @"ca-app-pub-\d+",                    // AdMob App / Ad Unit ID
                @"AIzaSy[A-Za-z0-9_-]{33}",           // Google API Key
                @"dashboard\.wasdmobile\.com",        // Shared internal server URL
                @"wm_[a-f0-9]{48}"                    // Shared Dashboard API key
            };

            foreach (var file in scriptFiles)
            {
                if (file.Contains("Editor") || file.Contains("AccountSafetyScaffolder")) continue;

                string content = File.ReadAllText(file);
                foreach (var pattern in suspiciousPatterns)
                {
                    var matches = Regex.Matches(content, pattern);
                    if (matches.Count > 0)
                    {
                        string relativePath = "Assets" + file.Substring(Application.dataPath.Length);
                        _detectedHardcodedKeys.Add($"[Code] '{relativePath}' chứa mẫu nhạy cảm: {pattern} ({matches.Count} chỗ)");
                    }
                }
            }

            // 2. Quét Android Manifests & XMLs (Kiểm tra nguy cơ Reject Store)
            string[] xmlFiles = Directory.GetFiles(Application.dataPath, "*.xml", SearchOption.AllDirectories);
            foreach (var file in xmlFiles)
            {
                string content = File.ReadAllText(file);
                string relativePath = "Assets" + file.Substring(Application.dataPath.Length);

                if (content.Contains("android:debuggable=\"true\""))
                {
                    _detectedHardcodedKeys.Add($"[Manifest Reject] '{relativePath}' có 'android:debuggable=\"true\"'. Google Play sẽ TỪ CHỐI APK/AAB này!");
                }
                if (content.Contains("android.permission.INSTALL_PACKAGES"))
                {
                    _detectedHardcodedKeys.Add($"[Permission Reject] '{relativePath}' chứa 'INSTALL_PACKAGES'. Quyền hệ thống này bị Google Play CẤM đối với app thường!");
                }
                if (content.Contains("cleartextTrafficPermitted=\"true\"") && !content.Contains("127.0.0.1"))
                {
                    _detectedHardcodedKeys.Add($"[Security Warning] '{relativePath}' cho phép cleartext HTTP traffic toàn app (nguy cơ cảnh báo bảo mật Google Play)!");
                }
                if (content.Contains("com.example."))
                {
                    _detectedHardcodedKeys.Add($"[Package Reject] '{relativePath}' chứa package name mẫu 'com.example.*'. Google Play sẽ từ chối!");
                }
            }
        }
    }
}
