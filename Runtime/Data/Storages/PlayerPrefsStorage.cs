using System;
using UnityEngine;
using Unity.Core.Logging;

namespace Unity.Core.Data
{
    /// <summary>
    /// Lưu trữ dữ liệu game qua PlayerPrefs, hỗ trợ tùy chọn mã hóa chống gian lận.
    /// </summary>
    public class PlayerPrefsStorage<T> : IDataStorage<T> where T : class, new()
    {
        public string Key { get; }
        public bool Encrypt { get; }

        public PlayerPrefsStorage(string key, bool encrypt = false)
        {
            Key = key;
            Encrypt = encrypt;
        }

        public T LoadData()
        {
            string raw = PlayerPrefs.GetString(Key, string.Empty);
            if (string.IsNullOrEmpty(raw))
            {
                return new T();
            }

            if (Encrypt)
            {
                raw = DataCrypto.Decrypt(raw);
            }

            try
            {
                T result = JsonUtility.FromJson<T>(raw);
                return result ?? new T();
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"[PlayerPrefsStorage] Lỗi parse JSON cho key '{Key}': {ex.Message}");
                return new T();
            }
        }

        public void Save(T data)
        {
            if (data == null) return;

            string json = JsonUtility.ToJson(data);
            if (Encrypt)
            {
                json = DataCrypto.Encrypt(json);
            }

            PlayerPrefs.SetString(Key, json);
            PlayerPrefs.Save();
        }

        public bool HasData() => PlayerPrefs.HasKey(Key);

        public void Delete()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }
    }
}
