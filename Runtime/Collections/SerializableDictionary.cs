using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Core.Collections
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DictionaryAttribute : PropertyAttribute
    {
        public string KeyLabel { get; set; }
        public string ValueLabel { get; set; }
        public float KeySize { get; set; }

        public DictionaryAttribute(float keySize = 0.4f, string keyLabel = "Key", string valueLabel = "Value")
        {
            KeySize = Mathf.Clamp(keySize, 0.1f, 0.9f);
            KeyLabel = keyLabel;
            ValueLabel = valueLabel;
        }
    }

    /// <summary>
    /// Generic Dictionary có thể Serialize và hiển thị trực tiếp trong Unity Inspector thông qua ISerializationCallbackReceiver.
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        private List<TValue> values = new List<TValue>();

        public List<TKey> SerializedKeys => keys;
        public List<TValue> SerializedValues => values;

        public SerializableDictionary() : base() { }
        public SerializableDictionary(IDictionary<TKey, TValue> dict) : base(dict) { }
        public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (var kvp in this)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();

            int count = Math.Min(keys.Count, values.Count);
            for (int i = 0; i < count; i++)
            {
                if (keys[i] != null && !ContainsKey(keys[i]))
                {
                    this[keys[i]] = values[i];
                }
            }
        }
    }

    /// <summary>
    /// Reorderable Dictionary với cơ chế serialization tương tự SerializableDictionary.
    /// </summary>
    [Serializable]
    public class ReorderableDictionary<TKey, TValue> : SerializableDictionary<TKey, TValue>
    {
        public ReorderableDictionary() : base() { }
        public ReorderableDictionary(IDictionary<TKey, TValue> dict) : base(dict) { }
    }

    /// <summary>
    /// Generic Set có thể Serialize trong Unity Inspector.
    /// </summary>
    [Serializable]
    public class SerializableSet<T> : HashSet<T>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<T> values = new List<T>();

        public List<T> SerializedValues => values;

        public SerializableSet() : base() { }
        public SerializableSet(IEnumerable<T> collection) : base(collection) { }
        public SerializableSet(IEqualityComparer<T> comparer) : base(comparer) { }

        public void OnBeforeSerialize()
        {
            values.Clear();
            foreach (var item in this)
            {
                values.Add(item);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] != null)
                {
                    Add(values[i]);
                }
            }
        }
    }
}
