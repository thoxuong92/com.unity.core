using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Unity.Core.Logging;

namespace Unity.Core.Data
{
    /// <summary>
    /// Tiện ích mã hóa / giải mã dữ liệu save game (AES-256) giúp chống can thiệp, sửa đổi file save (Anti-Cheat).
    /// </summary>
    public static class DataCrypto
    {
        private static byte[] _customKey;
        private static byte[] _customIV;

        /// <summary>
        /// Cho phép thiết lập khóa mã hóa tùy chỉnh riêng cho từng game / dự án.
        /// </summary>
        public static void SetCustomKey(string key, string iv)
        {
            if (!string.IsNullOrEmpty(key))
            {
                using (var sha = SHA256.Create())
                {
                    _customKey = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
                }
            }
            if (!string.IsNullOrEmpty(iv))
            {
                using (var md5 = MD5.Create())
                {
                    _customIV = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                }
            }
        }

        private static byte[] GetKey()
        {
            if (_customKey != null) return _customKey;
            
            // Tự động sinh khóa mã hóa riêng biệt theo Package Name / Bundle ID của từng game
            // Đảm bảo không game nào bị trùng dấu vân tay (Anti-Fingerprint) giữa các tài khoản Store
            string appId = Application.identifier;
            if (string.IsNullOrEmpty(appId)) appId = Application.productName ?? "DefaultGameApp";
            
            using (var sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes($"CORE_{appId}_SALT_#2026"));
            }
        }

        private static byte[] GetIV()
        {
            if (_customIV != null) return _customIV;

            string appId = Application.identifier;
            if (string.IsNullOrEmpty(appId)) appId = Application.productName ?? "DefaultGameApp";

            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(Encoding.UTF8.GetBytes($"IV_{appId}"));
            }
        }

        /// <summary>
        /// Mã hóa chuỗi văn bản (JSON) sang chuỗi Base64 đã được mã hóa AES.
        /// </summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = GetKey();
                    aes.IV = GetIV();

                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            using (var sw = new StreamWriter(cs, Encoding.UTF8))
                            {
                                sw.Write(plainText);
                            }
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"[DataCrypto] Encryption error: {ex.Message}");
                return plainText;
            }
        }

        /// <summary>
        /// Giải mã chuỗi Base64 đã mã hóa AES trở lại chuỗi văn bản (JSON) ban đầu.
        /// </summary>
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (var aes = Aes.Create())
                {
                    aes.Key = GetKey();
                    aes.IV = GetIV();

                    using (var ms = new MemoryStream(cipherBytes))
                    {
                        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (var sr = new StreamReader(cs, Encoding.UTF8))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fallback nếu chuỗi truyền vào là plain JSON chưa mã hóa
                return cipherText;
            }
        }
    }
}
