using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Unity.Core.Logging;

namespace Unity.Core.Data
{
    /// <summary>
    /// Tiện ích mã hóa / giải mã dữ liệu save game (AES-256) giúp chống can thiệp, sửa đổi file save (Anti-Cheat).
    /// </summary>
    public static class DataCrypto
    {
        private static readonly byte[] DefaultKey = Encoding.UTF8.GetBytes("UNITY_CORE_SECURE_KEY_2026_V1.0!");
        private static readonly byte[] DefaultIV = Encoding.UTF8.GetBytes("UNITY_INIT_VECT!");

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
                    aes.Key = DefaultKey;
                    aes.IV = DefaultIV;

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
                    aes.Key = DefaultKey;
                    aes.IV = DefaultIV;

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
