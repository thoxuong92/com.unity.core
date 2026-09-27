using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Unity.Core.Helpers
{
    public static class CryptoHelper
    {
        private static readonly byte[] DefaultKey = Encoding.UTF8.GetBytes("UnityCoreFrameworkSecureKey2026!");
        private static readonly byte[] DefaultIV = Encoding.UTF8.GetBytes("UnityCoreIV_2026");

        public static string Encrypt(string plainText, byte[] key = null, byte[] iv = null)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            key ??= DefaultKey;
            iv ??= DefaultIV;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        public static string Decrypt(string cipherText, byte[] key = null, byte[] iv = null)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            key ??= DefaultKey;
            iv ??= DefaultIV;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                {
                    byte[] cipherBytes = Convert.FromBase64String(cipherText);
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
    }

    public static class TimeHelper
    {
        public static string FormatDate(DateTime dt, char separator = '/')
        {
            return dt.ToString($"dd{separator}MM{separator}yyyy");
        }

        public static string FormatTime(DateTime dt, char separator = ':')
        {
            return dt.ToString($"HH{separator}mm{separator}ss");
        }

        public static int SecondToMinute(int seconds)
        {
            return seconds / 60;
        }

        public static string FormatDuration(float seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            return t.Hours > 0 ? $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}" : $"{t.Minutes:D2}:{t.Seconds:D2}";
        }
    }
}
