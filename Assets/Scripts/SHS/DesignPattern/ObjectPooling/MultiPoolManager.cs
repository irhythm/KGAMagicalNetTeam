using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class MultiPoolManager<T> : ObjectPoolManager where T : Component
{
    // 오브젝트를 담을 풀
    private Dictionary<string, IObjectPool<T>> pool = new Dictionary<string, IObjectPool<T>>();

    [Header("초기에 생성할 객체 자료구조")]
    [SerializeField] private T[] datas;     // 초기에 미리 생성해둘게 있다면 담아둘 배열

    private void Awake()
    {
        if(datas != null && datas.Length > 0 && initCount > 0)
        {
            foreach(var data in datas)
            {
                for(int i = 0; i < initCount; i++)
                {
                    Get(data);
                }
            }
        }
    }

    public T Get(T data)
    {
        if(!pool.ContainsKey(data.name))
        {
            pool[data.name] = new ObjectPool<T>(
                createFunc: CreateObject(data),
                actionOnGet: ActivatePoolObject,
                actionOnRelease: DisablePoolObject,
                actionOnDestroy: DestroyPoolObject,
                collectionCheck: false,
                defaultCapacity: initSize,
                maxSize: maxSize
                );
        }

        return pool[data.name].Get();
    }

    public void Release(T data)
    {
        if(pool.ContainsKey(data.name))
        {
            pool[data.name].Release(data);
        }
        else
        {
            Destroy(data.gameObject);
        }
    }

    // 생성할 때
    protected virtual Func<T> CreateObject(T obj)
    {
        return () => Instantiate(obj, transform);
    }

    // 활성화할 때
    protected virtual void ActivatePoolObject(T obj)
    {
        obj.gameObject.SetActive(true);
    }

    // 비활성화할 때
    protected virtual void DisablePoolObject(T obj)
    {
        obj.gameObject.SetActive(false);
    }

    // 파괴할 때
    protected virtual void DestroyPoolObject(T obj)
    {
        Destroy(obj.gameObject);
    }
}
