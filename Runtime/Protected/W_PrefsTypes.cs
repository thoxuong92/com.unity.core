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
    public class W_Int : IProtectedPref<int>
    {
        [SerializeField] private string key;
        [SerializeField] private int defaultValue;

        public string Key => key;
        public int Value
        {
            get => W_PlayerPrefs.GetInt(key, defaultValue);
            set => W_PlayerPrefs.SetInt(key, value);
        }

        public W_Int() { }
        public W_Int(string key, int defaultValue = 0)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public static implicit operator int(W_Int pref) => pref != null ? pref.Value : 0;
    }

    [Serializable]
    public class W_Float : IProtectedPref<float>
    {
        [SerializeField] private string key;
        [SerializeField] private float defaultValue;

        public string Key => key;
        public float Value
        {
            get => W_PlayerPrefs.GetFloat(key, defaultValue);
            set => W_PlayerPrefs.SetFloat(key, value);
        }

        public W_Float() { }
        public W_Float(string key, float defaultValue = 0f)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public static implicit operator float(W_Float pref) => pref != null ? pref.Value : 0f;
    }

    [Serializable]
    public class W_Bool : IProtectedPref<bool>
    {
        [SerializeField] private string key;
        [SerializeField] private bool defaultValue;

        public string Key => key;
        public bool Value
        {
            get => W_PlayerPrefs.GetBool(key, defaultValue);
            set => W_PlayerPrefs.SetBool(key, value);
        }

        public W_Bool() { }
        public W_Bool(string key, bool defaultValue = false)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public static implicit operator bool(W_Bool pref) => pref != null && pref.Value;
    }

    [Serializable]
    public class W_String : IProtectedPref<string>
    {
        [SerializeField] private string key;
        [SerializeField] private string defaultValue;

        public string Key => key;
        public string Value
        {
            get => W_PlayerPrefs.GetString(key, defaultValue);
            set => W_PlayerPrefs.SetString(key, value);
        }

        public W_String() { }
        public W_String(string key, string defaultValue = "")
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public static implicit operator string(W_String pref) => pref?.Value;
    }

    [Serializable]
    public class W_Vector2 : IProtectedPref<Vector2>
    {
        [SerializeField] private string key;
        [SerializeField] private Vector2 defaultValue;

        public string Key => key;
        public Vector2 Value
        {
            get => W_PlayerPrefs.GetVector2(key, defaultValue);
            set => W_PlayerPrefs.SetVector2(key, value);
        }

        public W_Vector2() { }
        public W_Vector2(string key, Vector2 defaultValue = default)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }
    }

    [Serializable]
    public class W_Vector3 : IProtectedPref<Vector3>
    {
        [SerializeField] private string key;
        [SerializeField] private Vector3 defaultValue;

        public string Key => key;
        public Vector3 Value
        {
            get => W_PlayerPrefs.GetVector3(key, defaultValue);
            set => W_PlayerPrefs.SetVector3(key, value);
        }

        public W_Vector3() { }
        public W_Vector3(string key, Vector3 defaultValue = default)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }
    }

    [Serializable]
    public class W_Vector4 : IProtectedPref<Vector4>
    {
        [SerializeField] private string key;
        [SerializeField] private Vector4 defaultValue;

        public string Key => key;
        public Vector4 Value
        {
            get => W_PlayerPrefs.GetVector4(key, defaultValue);
            set => W_PlayerPrefs.SetVector4(key, value);
        }

        public W_Vector4() { }
        public W_Vector4(string key, Vector4 defaultValue = default)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }
    }

    [Serializable]
    public class W_Quaternion : IProtectedPref<Quaternion>
    {
        [SerializeField] private string key;
        [SerializeField] private Quaternion defaultValue;

        public string Key => key;
        public Quaternion Value
        {
            get => W_PlayerPrefs.GetQuaternion(key, defaultValue);
            set => W_PlayerPrefs.SetQuaternion(key, value);
        }

        public W_Quaternion() { }
        public W_Quaternion(string key, Quaternion defaultValue = default)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }
    }
}
