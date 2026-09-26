using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Unity.Core.Protected
{
    /// <summary>
    /// Memory-obfuscated Integer to prevent memory tampering (Cheat Engine, GameGuardian, LuckyPatcher).
    /// Stores an XOR-masked value and a randomized salt key in memory.
    /// </summary>
    [Serializable]
    public struct ProtectedInt : IEquatable<ProtectedInt>
    {
        [SerializeField] private int _encryptedValue;
        [SerializeField] private int _cryptoKey;

        public ProtectedInt(int value)
        {
            _cryptoKey = Random.Range(10000, 99999);
            _encryptedValue = value ^ _cryptoKey;
        }

        public int Value
        {
            get => _encryptedValue ^ _cryptoKey;
            set
            {
                _cryptoKey = Random.Range(10000, 99999);
                _encryptedValue = value ^ _cryptoKey;
            }
        }

        public static implicit operator int(ProtectedInt val) => val.Value;
        public static implicit operator ProtectedInt(int val) => new ProtectedInt(val);

        public static ProtectedInt operator ++(ProtectedInt val)
        {
            val.Value += 1;
            return val;
        }

        public static ProtectedInt operator --(ProtectedInt val)
        {
            val.Value -= 1;
            return val;
        }

        public override string ToString() => Value.ToString();
        public override bool Equals(object obj) => obj is ProtectedInt other && Equals(other);
        public bool Equals(ProtectedInt other) => Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
    }

    /// <summary>
    /// Memory-obfuscated Float to prevent memory tampering.
    /// </summary>
    [Serializable]
    public struct ProtectedFloat : IEquatable<ProtectedFloat>
    {
        [SerializeField] private int _encryptedValue;
        [SerializeField] private int _cryptoKey;

        public ProtectedFloat(float value)
        {
            _cryptoKey = Random.Range(10000, 99999);
            byte[] bytes = BitConverter.GetBytes(value);
            int rawInt = BitConverter.ToInt32(bytes, 0);
            _encryptedValue = rawInt ^ _cryptoKey;
        }

        public float Value
        {
            get
            {
                int rawInt = _encryptedValue ^ _cryptoKey;
                byte[] bytes = BitConverter.GetBytes(rawInt);
                return BitConverter.ToSingle(bytes, 0);
            }
            set
            {
                _cryptoKey = Random.Range(10000, 99999);
                byte[] bytes = BitConverter.GetBytes(value);
                int rawInt = BitConverter.ToInt32(bytes, 0);
                _encryptedValue = rawInt ^ _cryptoKey;
            }
        }

        public static implicit operator float(ProtectedFloat val) => val.Value;
        public static implicit operator ProtectedFloat(float val) => new ProtectedFloat(val);

        public override string ToString() => Value.ToString();
        public override bool Equals(object obj) => obj is ProtectedFloat other && Equals(other);
        public bool Equals(ProtectedFloat other) => Mathf.Approximately(Value, other.Value);
        public override int GetHashCode() => Value.GetHashCode();
    }

    /// <summary>
    /// Memory-obfuscated Boolean.
    /// </summary>
    [Serializable]
    public struct ProtectedBool : IEquatable<ProtectedBool>
    {
        [SerializeField] private byte _encryptedValue;
        [SerializeField] private byte _cryptoKey;

        public ProtectedBool(bool value)
        {
            _cryptoKey = (byte)Random.Range(1, 255);
            byte raw = (byte)(value ? 1 : 0);
            _encryptedValue = (byte)(raw ^ _cryptoKey);
        }

        public bool Value
        {
            get => ((byte)(_encryptedValue ^ _cryptoKey)) == 1;
            set
            {
                _cryptoKey = (byte)Random.Range(1, 255);
                byte raw = (byte)(value ? 1 : 0);
                _encryptedValue = (byte)(raw ^ _cryptoKey);
            }
        }

        public static implicit operator bool(ProtectedBool val) => val.Value;
        public static implicit operator ProtectedBool(bool val) => new ProtectedBool(val);

        public override string ToString() => Value.ToString();
        public override bool Equals(object obj) => obj is ProtectedBool other && Equals(other);
        public bool Equals(ProtectedBool other) => Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
    }
}
