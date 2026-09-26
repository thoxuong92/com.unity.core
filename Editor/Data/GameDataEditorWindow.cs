using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.Core.Data;

namespace Unity.Core.Editor.Data
{
    /// <summary>
    /// Cửa sổ quản lý và xem trước dữ liệu Save Game trong Unity Editor.
    /// Cho phép xem JSON, chỉnh sửa thông số trực tiếp (Coins, Level, Sound...), sao lưu và xóa dữ liệu test.
    /// </summary>
    public class GameDataEditorWindow : EditorWindow
    {
        private string _targetKey = PlayerDataHandler.DefaultKey;
        private string _rawJson = "";
        private PlayerProfile _cachedProfile = new PlayerProfile();
        private Vector2 _scrollPos;
        private bool _useEncryption = false;
        private StorageType _selectedStorage = StorageType.PlayerPrefs;

        private enum StorageType
        {
            PlayerPrefs,
            PersistentFile
        }

        [MenuItem("Unity Core/Game Data Manager", false, 30)]
        public static void OpenWindow()
        {
            var window = GetWindow<GameDataEditorWindow>("Game Data Manager");
            window.minSize = new Vector2(480, 420);
            window.Show();
        }

        private void OnEnable()
        {
            ReloadData();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("<b>🎮 GAME DATA & SAVE MANAGER</b>", new GUIStyle(EditorStyles.boldLabel) { richText = true, fontSize = 14 });
            EditorGUILayout.HelpBox("Quản lý, xem và chỉnh sửa trực tiếp dữ liệu lưu trữ (Save Data) của người chơi trong Editor.", MessageType.Info);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(6);

            // Storage Config
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _selectedStorage = (StorageType)EditorGUILayout.EnumPopup("Loại lưu trữ (Storage):", _selectedStorage);
            _targetKey = EditorGUILayout.TextField("Tên Key / File:", _targetKey);
            _useEncryption = EditorGUILayout.Toggle("Có mã hóa AES:", _useEncryption);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Tải lại dữ liệu (Reload)", EditorStyles.miniButton))
            {
                ReloadData();
            }
            if (GUILayout.Button("📂 Mở Thư Mục Persistent Data", EditorStyles.miniButton))
            {
                EditorUtility.RevealInFinder(Application.persistentDataPath);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            // Profile Fields Editor
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("<b>Hồ Sơ Người Chơi (PlayerProfile):</b>", new GUIStyle(EditorStyles.boldLabel) { richText = true });

            _cachedProfile.PlayerName = EditorGUILayout.TextField("Tên người chơi:", _cachedProfile.PlayerName);
            _cachedProfile.Level = EditorGUILayout.IntField("Level:", _cachedProfile.Level);
            _cachedProfile.Coins = EditorGUILayout.LongField("Coins:", _cachedProfile.Coins);
            _cachedProfile.Gems = EditorGUILayout.IntField("Gems:", _cachedProfile.Gems);
            _cachedProfile.HighScore = EditorGUILayout.IntField("High Score:", _cachedProfile.HighScore);

            EditorGUILayout.Space(4);
            _cachedProfile.Sound = EditorGUILayout.Toggle("Âm thanh (Sound):", _cachedProfile.Sound);
            _cachedProfile.Music = EditorGUILayout.Toggle("Nhạc nền (Music):", _cachedProfile.Music);
            _cachedProfile.Haptics = EditorGUILayout.Toggle("Rung (Haptics):", _cachedProfile.Haptics);
            _cachedProfile.Light = EditorGUILayout.Toggle("Ánh sáng (Light):", _cachedProfile.Light);
            _cachedProfile.MasterVolume = EditorGUILayout.Slider("Âm lượng tổng:", _cachedProfile.MasterVolume, 0f, 1f);

            EditorGUILayout.Space(6);

            // Actions
            EditorGUILayout.BeginHorizontal();
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.3f, 0.9f, 0.4f);
            if (GUILayout.Button("💾 Lưu Thay Đổi (Save)", GUILayout.Height(30)))
            {
                SaveCurrentData();
            }
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("🗑 Xóa Dữ Liệu (Delete)", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Xác nhận xóa", $"Bạn có chắc chắn muốn xóa toàn bộ dữ liệu lưu của '{_targetKey}' không?", "Xóa", "Hủy"))
                {
                    DeleteData();
                }
            }
            GUI.backgroundColor = oldColor;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            // Raw JSON View
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("<b>Dữ liệu JSON thô:</b>", new GUIStyle(EditorStyles.boldLabel) { richText = true });
            EditorGUILayout.TextArea(_rawJson, GUILayout.Height(100));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📋 Copy JSON", EditorStyles.miniButton))
            {
                EditorGUIUtility.systemCopyBuffer = _rawJson;
                Debug.Log("[Game Data Manager] Đã copy JSON vào Clipboard!");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void ReloadData()
        {
            if (_selectedStorage == StorageType.PlayerPrefs)
            {
                var storage = new PlayerPrefsStorage<PlayerProfile>(_targetKey, _useEncryption);
                _cachedProfile = storage.LoadData();
            }
            else
            {
                var storage = new FileStorage<PlayerProfile>(_targetKey, _useEncryption);
                _cachedProfile = storage.LoadData();
            }

            _rawJson = JsonUtility.ToJson(_cachedProfile, true);
            Repaint();
        }

        private void SaveCurrentData()
        {
            if (_selectedStorage == StorageType.PlayerPrefs)
            {
                var storage = new PlayerPrefsStorage<PlayerProfile>(_targetKey, _useEncryption);
                storage.Save(_cachedProfile);
            }
            else
            {
                var storage = new FileStorage<PlayerProfile>(_targetKey, _useEncryption);
                storage.Save(_cachedProfile);
            }

            ReloadData();
            Debug.Log($"[Game Data Manager] Đã lưu thành công dữ liệu cho '{_targetKey}'!");
        }

        private void DeleteData()
        {
            if (_selectedStorage == StorageType.PlayerPrefs)
            {
                var storage = new PlayerPrefsStorage<PlayerProfile>(_targetKey, _useEncryption);
                storage.Delete();
            }
            else
            {
                var storage = new FileStorage<PlayerProfile>(_targetKey, _useEncryption);
                storage.Delete();
            }

            ReloadData();
            Debug.Log($"[Game Data Manager] Đã xóa dữ liệu cho '{_targetKey}'!");
        }
    }
}
