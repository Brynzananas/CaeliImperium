using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Misc;
[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = [];
    [SerializeField] private List<TValue> values = [];
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }
    public void OnAfterDeserialize()
    {
        this.Clear();
        if (keys.Count != values.Count)
        {
            CaeliImperiumPlugin.Log.LogError($"Deserialization error: keys count ({keys.Count}) does not match values count ({values.Count}).");
            return;
        }
        for (int i = 0; i < keys.Count; i++)
        {
            if (!this.ContainsKey(keys[i]))
            {
                this.Add(keys[i], values[i]);
            }
        }
    }
}