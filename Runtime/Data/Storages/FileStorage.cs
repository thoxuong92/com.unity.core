using System;
using System.IO;
using UnityEngine;
using Unity.Core.Logging;

namespace Unity.Core.Data
{
    /// <summary>
    /// Lưu trữ dữ liệu game thành các file JSON trong Application.persistentDataPath.
    /// Vượt qua giới hạn dung lượng của PlayerPrefs, hỗ trợ ghi file an toàn (Atomic File Write) và tự động Backup (.bak).
    /// </summary>
    public class FileStorage<T> : IDataStorage<T> where T : class, new()
    {
        public string FileName { get; }
        public bool Encrypt { get; }
        public string FilePath { get; }
        public string BackupPath { get; }

        public FileStorage(string fileName, bool encrypt = false)
        {
            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".json";
            }

            FileName = fileName;
            Encrypt = encrypt;
            FilePath = Path.Combine(Application.persistentDataPath, FileName);
            BackupPath = Path.Combine(Application.persistentDataPath, $"{FileName}.bak");
        }

        public T LoadData()
        {
            // 1. Thử đọc file chính
            if (File.Exists(FilePath))
            {
                var data = ReadFromFile(FilePath);
                if (data != null) return data;
            }

            // 2. Nếu file chính hỏng, phục hồi từ file Backup
            if (File.Exists(BackupPath))
            {
                AppLogger.LogWarning($"[FileStorage] File chính '{FileName}' bị hỏng, đang khôi phục từ file Backup...");
                var data = ReadFromFile(BackupPath);
                if (data != null) return data;
            }

            return new T();
        }

        private T ReadFromFile(string path)
        {
            try
            {
                string raw = File.ReadAllText(path);
                if (string.IsNullOrEmpty(raw)) return null;

                if (Encrypt)
                {
                    raw = DataCrypto.Decrypt(raw);
                }

                return JsonUtility.FromJson<T>(raw);
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"[FileStorage] Lỗi đọc file '{path}': {ex.Message}");
                return null;
            }
        }

        public void Save(T data)
        {
            if (data == null) return;
            string json = JsonUtility.ToJson(data, true);
            Save(json);
        }

        public void Save(string rawJson)
        {
            try
            {
                string textToWrite = Encrypt ? DataCrypto.Encrypt(rawJson) : rawJson;
                string tempPath = Path.Combine(Application.persistentDataPath, $"{FileName}.tmp");

                // Ghi vào file tạm trước để chống hỏng file nếu crash giữa chừng
                File.WriteAllText(tempPath, textToWrite);

                // Tạo backup file cũ
                if (File.Exists(FilePath))
                {
                    File.Copy(FilePath, BackupPath, true);
                }

                // Ghi đè file chính
                File.Copy(tempPath, FilePath, true);
                File.Delete(tempPath);
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"[FileStorage] Lỗi lưu file '{FilePath}': {ex.Message}");
            }
        }

        public bool HasData() => File.Exists(FilePath) || File.Exists(BackupPath);

        public void Delete()
        {
            try
            {
                if (File.Exists(FilePath)) File.Delete(FilePath);
                if (File.Exists(BackupPath)) File.Delete(BackupPath);
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"[FileStorage] Lỗi xóa file '{FilePath}': {ex.Message}");
            }
        }
    }
}
