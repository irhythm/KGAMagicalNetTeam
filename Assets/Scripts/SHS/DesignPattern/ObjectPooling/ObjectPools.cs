using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 데이터를 오브젝트 풀로 관리하는 클래스
/// 오브젝트 풀로 관리할 데이터가 여러 종류일 경우 사용
/// </summary>
/// <typeparam name="T"> 풀로 관리할 객체 타입 </typeparam>
public class ObjectPools<T> : Singleton<ObjectPools<T>> where T : MonoBehaviour
{
    protected Dictionary<string, Queue<T>> pool = new Dictionary<string, Queue<T>>();

    protected override void Awake()
    {
        base.Awake();
    }

    public T GetObject(T type, Transform trans)
    {
        string name = type.name;
        T data;

        if (!pool.ContainsKey(name))
        {
            pool.Add(name, new Queue<T>());
        }

        if (pool[name].Count == 0)
        {
            data = Instantiate(type, trans);
            data.name = name;
        }
        else
        {
            data = pool[name].Dequeue();
            data.gameObject.SetActive(true);
        }

        return data;
    }

    public void TakeObject(T data)
    {
        if (data == null) return;

        string name = data.name;

        if (!pool.ContainsKey(name))
        {
            pool.Add(name, new Queue<T>());
        }

        data.gameObject.SetActive(false);
        pool[name].Enqueue(data);
    }
}
