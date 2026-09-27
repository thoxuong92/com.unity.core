using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Unity.Core.Protected
{
    /// <summary>
    /// PlayerPrefs mã hóa và chống can thiệp (Anti-Cheat Obfuscated PlayerPrefs).
    /// Tự động sinh khóa băm và chữ ký toàn vẹn (integrity hash) để ngăn chặn việc sửa đổi file save từ bên ngoài.
    /// Hỗ trợ thêm các kiểu dữ liệu phong phú: bool, Vector2, Vector3, Vector4, Quaternion.
    /// </summary>
    public static class W_PlayerPrefs
    {
        private const string HashSuffix = "_integrity_hash";
        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("UnityCoreTamperShield2026");

        public static bool AutoSave { get; set; } = true;
        public static bool IsProtectedEnabled { get; set; } = true;

        private static string GetProtectedKey(string key)
        {
            if (!IsProtectedEnabled) return key;
            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(key + "UnityCoreKeySalt"));
            return "p_" + BitConverter.ToString(hash).Replace("-", "").Substring(0, 16);
        }

        private static string GetHashKey(string key)
        {
            return GetProtectedKey(key) + HashSuffix;
        }

        private static int ComputeIntegrityHash(string key, string valueStr)
        {
            using var sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(key + valueStr + BitConverter.ToString(Salt));
            byte[] hash = sha.ComputeHash(bytes);
            return BitConverter.ToInt32(hash, 0);
        }

        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(GetProtectedKey(key)) || PlayerPrefs.HasKey(key);
        }

        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(GetProtectedKey(key));
            PlayerPrefs.DeleteKey(GetHashKey(key));
            if (PlayerPrefs.HasKey(key)) PlayerPrefs.DeleteKey(key);
        }

        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }

        #region Int
        public static void SetInt(string key, int value)
        {
            string pKey = GetProtectedKey(key);
            int saltKey = 0x5F3759DF ^ key.GetHashCode();
            int obfuscated = value ^ saltKey;

            PlayerPrefs.SetInt(pKey, obfuscated);
            PlayerPrefs.SetInt(GetHashKey(key), ComputeIntegrityHash(key, value.ToString()));

            if (AutoSave) Save();
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            string pKey = GetProtectedKey(key);
            if (PlayerPrefs.HasKey(pKey))
            {
                int saltKey = 0x5F3759DF ^ key.GetHashCode();
                int obfuscated = PlayerPrefs.GetInt(pKey);
                int val = obfuscated ^ saltKey;

                int savedHash = PlayerPrefs.GetInt(GetHashKey(key), 0);
                if (savedHash == ComputeIntegrityHash(key, val.ToString()))
                {
                    return val;
                }
                Debug.LogWarning($"[W_PlayerPrefs] Phát hiện can thiệp dữ liệu cho key '{key}'! Giá trị trả về mặc định.");
                return defaultValue;
            }

            return PlayerPrefs.GetInt(key, defaultValue);
        }
        #endregion

        #region Float
        public static void SetFloat(string key, float value)
        {
            SetString(key, value.ToString("R"));
        }

        public static float GetFloat(string key, float defaultValue = 0f)
        {
            string str = GetString(key, null);
            if (str != null && float.TryParse(str, out float result))
            {
                return result;
            }
            return PlayerPrefs.GetFloat(key, defaultValue);
        }
        #endregion

        #region Bool
        public static void SetBool(string key, bool value)
        {
            SetInt(key, value ? 1 : 0);
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            return GetInt(key, defaultValue ? 1 : 0) == 1;
        }
        #endregion

        #region String
        public static void SetString(string key, string value)
        {
            string pKey = GetProtectedKey(key);
            string enc = Helpers.CryptoHelper.Encrypt(value ?? "");

            PlayerPrefs.SetString(pKey, enc);
            PlayerPrefs.SetInt(GetHashKey(key), ComputeIntegrityHash(key, value ?? ""));

            if (AutoSave) Save();
        }

        public static string GetString(string key, string defaultValue = "")
        {
            string pKey = GetProtectedKey(key);
            if (PlayerPrefs.HasKey(pKey))
            {
                string enc = PlayerPrefs.GetString(pKey);
                string plain = Helpers.CryptoHelper.Decrypt(enc);

                int savedHash = PlayerPrefs.GetInt(GetHashKey(key), 0);
                if (savedHash == ComputeIntegrityHash(key, plain))
                {
                    return plain;
                }
                Debug.LogWarning($"[W_PlayerPrefs] Phát hiện can thiệp string cho key '{key}'!");
                return defaultValue;
            }

            return PlayerPrefs.GetString(key, defaultValue);
        }
        #endregion

        #region Vector & Quaternion
        public static void SetVector2(string key, Vector2 value)
        {
            SetString(key, $"{value.x}|{value.y}");
        }

        public static Vector2 GetVector2(string key, Vector2 defaultValue = default)
        {
            string s = GetString(key, null);
            if (!string.IsNullOrEmpty(s))
            {
                string[] parts = s.Split('|');
                if (parts.Length == 2 && float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y))
                {
                    return new Vector2(x, y);
                }
            }
            return defaultValue;
        }

        public static void SetVector3(string key, Vector3 value)
        {
            SetString(key, $"{value.x}|{value.y}|{value.z}");
        }

        public static Vector3 GetVector3(string key, Vector3 defaultValue = default)
        {
            string s = GetString(key, null);
            if (!string.IsNullOrEmpty(s))
            {
                string[] parts = s.Split('|');
                if (parts.Length == 3 && float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y) && float.TryParse(parts[2], out float z))
                {
                    return new Vector3(x, y, z);
                }
            }
            return defaultValue;
        }

        public static void SetVector4(string key, Vector4 value)
        {
            SetString(key, $"{value.x}|{value.y}|{value.z}|{value.w}");
        }

        public static Vector4 GetVector4(string key, Vector4 defaultValue = default)
        {
            string s = GetString(key, null);
            if (!string.IsNullOrEmpty(s))
            {
                string[] parts = s.Split('|');
                if (parts.Length == 4 && float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y) && float.TryParse(parts[2], out float z) && float.TryParse(parts[3], out float w))
                {
                    return new Vector4(x, y, z, w);
                }
            }
            return defaultValue;
        }

        public static void SetQuaternion(string key, Quaternion value)
        {
            SetVector4(key, new Vector4(value.x, value.y, value.z, value.w));
        }

        public static Quaternion GetQuaternion(string key, Quaternion defaultValue = default)
        {
            Vector4 v = GetVector4(key, new Vector4(defaultValue.x, defaultValue.y, defaultValue.z, defaultValue.w));
            return new Quaternion(v.x, v.y, v.z, v.w);
        }
        #endregion
    }
}
