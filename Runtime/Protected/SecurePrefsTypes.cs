using System;
using UnityEngine;

namespace Unity.Core.Protected
{
    public interface IProtectedPref<T>
    {
        string Key { get; }
        T Value { get; set; }
    }

    [Serializable]
    public class SecureInt : IProtectedPref<int>
    {
        [SerializeField] private string key;
        [SerializeField] private int defaultValue;

        public string Key => key;
        public int Value
        {
            get => SecurePlayerPrefs.GetInt(key, defaultValue);
            set => SecurePlayerPrefs.SetInt(key, value);
        }

        public SecureInt() { }
        public SecureInt(string key, int defaultValue = 0)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public static implicit operator int(SecureInt pref) => pref != null ? pref.Value : 0;
    }

    [Serializable]
    public class SecureFloat : IProtectedPref<float>
    {
        [SerializeField] private string key;
        [SerializeField] private float defaultValue;

        public string Key => key;
        public float Value
        {
            get => SecurePlayerPrefs.GetFloat(key, defaultValue);
            set => SecurePlayerPrefs.SetFloat(key, value);
        }

        public SecureFloat() { }
        public SecureFloat(string key, float defaultValue = 0f)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public static implicit operator float(SecureFloat pref) => pref != null ? pref.Value : 0f;
    }

    [Serializable]
    public class SecureBool : IProtectedPref<bool>
    {
        [SerializeField] private string key;
        [SerializeField] private bool defaultValue;

        public string Key => key;
        public bool Value
        {
            get => SecurePlayerPrefs.GetBool(key, defaultValue);
            set => SecurePlayerPrefs.SetBool(key, value);
        }

        public SecureBool() { }
        public SecureBool(string key, bool defaultValue = false)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public static implicit operator bool(SecureBool pref) => pref != null && pref.Value;
    }

    [Serializable]
    public class SecureString : IProtectedPref<string>
    {
        [SerializeField] private string key;
        [SerializeField] private string defaultValue;

        public string Key => key;
        public string Value
        {
            get => SecurePlayerPrefs.GetString(key, defaultValue);
            set => SecurePlayerPrefs.SetString(key, value);
        }

        public SecureString() { }
        public SecureString(string key, string defaultValue = "")
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public static implicit operator string(SecureString pref) => pref != null ? pref.Value : null;
    }

    [Serializable]
    public class SecureVector2 : IProtectedPref<Vector2>
    {
        [SerializeField] private string key;
        [SerializeField] private Vector2 defaultValue;

        public string Key => key;
        public Vector2 Value
        {
            get => SecurePlayerPrefs.GetVector2(key, defaultValue);
            set => SecurePlayerPrefs.SetVector2(key, value);
        }

        public SecureVector2() { }
        public SecureVector2(string key, Vector2 defaultValue = default)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public static implicit operator Vector2(SecureVector2 pref) => pref != null ? pref.Value : default;
    }

    [Serializable]
    public class SecureVector3 : IProtectedPref<Vector3>
    {
        [SerializeField] private string key;
        [SerializeField] private Vector3 defaultValue;

        public string Key => key;
        public Vector3 Value
        {
            get => SecurePlayerPrefs.GetVector3(key, defaultValue);
            set => SecurePlayerPrefs.SetVector3(key, value);
        }

        public SecureVector3() { }
        public SecureVector3(string key, Vector3 defaultValue = default)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public static implicit operator Vector3(SecureVector3 pref) => pref != null ? pref.Value : default;
    }

    [Serializable]
    public class SecureVector4 : IProtectedPref<Vector4>
    {
        [SerializeField] private string key;
        [SerializeField] private Vector4 defaultValue;

        public string Key => key;
        public Vector4 Value
        {
            get => SecurePlayerPrefs.GetVector4(key, defaultValue);
            set => SecurePlayerPrefs.SetVector4(key, value);
        }

        public SecureVector4() { }
        public SecureVector4(string key, Vector4 defaultValue = default)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public static implicit operator Vector4(SecureVector4 pref) => pref != null ? pref.Value : default;
    }

    [Serializable]
    public class SecureQuaternion : IProtectedPref<Quaternion>
    {
        [SerializeField] private string key;
        [SerializeField] private Quaternion defaultValue;

        public string Key => key;
        public Quaternion Value
        {
            get => SecurePlayerPrefs.GetQuaternion(key, defaultValue);
            set => SecurePlayerPrefs.SetQuaternion(key, value);
        }

        public SecureQuaternion() { }
        public SecureQuaternion(string key, Quaternion defaultValue = default)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public static implicit operator Quaternion(SecureQuaternion pref) => pref != null ? pref.Value : default;
    }

    #region Backward-compatible Obsolete Aliases
    [Obsolete("Vui lòng sử dụng SecureInt thay cho W_Int.")]
    [Serializable] public class W_Int : SecureInt { public W_Int() { } public W_Int(string key, int defaultValue = 0) : base(key, defaultValue) { } }

    [Obsolete("Vui lòng sử dụng SecureFloat thay cho W_Float.")]
    [Serializable] public class W_Float : SecureFloat { public W_Float() { } public W_Float(string key, float defaultValue = 0f) : base(key, defaultValue) { } }

    [Obsolete("Vui lòng sử dụng SecureBool thay cho W_Bool.")]
    [Serializable] public class W_Bool : SecureBool { public W_Bool() { } public W_Bool(string key, bool defaultValue = false) : base(key, defaultValue) { } }

    [Obsolete("Vui lòng sử dụng SecureString thay cho W_String.")]
    [Serializable] public class W_String : SecureString { public W_String() { } public W_String(string key, string defaultValue = "") : base(key, defaultValue) { } }

    [Obsolete("Vui lòng sử dụng SecureVector2 thay cho W_Vector2.")]
    [Serializable] public class W_Vector2 : SecureVector2 { public W_Vector2() { } public W_Vector2(string key, Vector2 defaultValue = default) : base(key, defaultValue) { } }

    [Obsolete("Vui lòng sử dụng SecureVector3 thay cho W_Vector3.")]
    [Serializable] public class W_Vector3 : SecureVector3 { public W_Vector3() { } public W_Vector3(string key, Vector3 defaultValue = default) : base(key, defaultValue) { } }

    [Obsolete("Vui lòng sử dụng SecureVector4 thay cho W_Vector4.")]
    [Serializable] public class W_Vector4 : SecureVector4 { public W_Vector4() { } public W_Vector4(string key, Vector4 defaultValue = default) : base(key, defaultValue) { } }

    [Obsolete("Vui lòng sử dụng SecureQuaternion thay cho W_Quaternion.")]
    [Serializable] public class W_Quaternion : SecureQuaternion { public W_Quaternion() { } public W_Quaternion(string key, Quaternion defaultValue = default) : base(key, defaultValue) { } }
    #endregion
}
