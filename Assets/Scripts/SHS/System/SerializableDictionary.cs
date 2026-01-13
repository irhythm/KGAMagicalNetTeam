using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class SerializableData<K, V> where K : class
{
    public K Key;
    public V Value;
}

[Serializable]
public class SerializableDictionary<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver where K : class
{
    [SerializeField] private List<SerializableData<K, V>> dataList = new List<SerializableData<K, V>>();

    public void OnBeforeSerialize()
    {
        if (this.Count < dataList.Count) return;

        dataList.Clear();

        foreach (var kv in this)
        {
            dataList.Add(new SerializableData<K, V>()
            {
                Key = kv.Key,
                Value = kv.Value
            });
        }
    }

    public void OnAfterDeserialize()
    {
        this.Clear();
        foreach(var kv in dataList)
        {
            if(!this.TryAdd(kv.Key, kv.Value))
            {
                Debug.LogError("List has duplicate key");
            }
        }
    }
}
