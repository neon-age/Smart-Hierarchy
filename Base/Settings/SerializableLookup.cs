using System;
using System.Collections.Generic;
using UnityEngine;

namespace AV.Hierarchy
{
    [Serializable]
    public class SerializableLookup<TKey, TValue> : ISerializationCallbackReceiver
    {
        public List<TKey> keys = new List<TKey>();
        public List<TValue> values = new List<TValue>();

        public Dictionary<TKey, TValue>  dictionary = new Dictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            get => dictionary[key];
            set => dictionary[key] = value;
        }

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (var kvp in dictionary)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            dictionary = new Dictionary<TKey, TValue>();

            for (int i = 0; i != Math.Min(keys.Count, values.Count); i++)
                dictionary.Add(keys[i], values[i]);
        }

        public bool Contains(TKey key)
        {
            return dictionary.ContainsKey(key);
        }
        
        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public void Add(TKey key, TValue value)
        {
            dictionary.Add(key, value);
        }
        
        public void Remove(TKey key)
        {
            dictionary.Remove(key);
        }
    }
}