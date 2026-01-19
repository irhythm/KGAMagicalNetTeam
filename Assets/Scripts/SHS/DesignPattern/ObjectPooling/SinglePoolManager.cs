using System;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 오브젝트 풀
/// Get(): 풀에서 오브젝트 꺼내기
/// Release(T Object): 풀에 오브젝트 넣기
/// Clear(): 풀의 모든 객체를 제거하고 초기화
/// CountActive() / CountInactive(): 현재 사용 중인 오브젝트 / 쉬고 있는 오브젝트의 수 확인
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SinglePoolManager<T> : ObjectPoolManager where T : Component
{
    // 오브젝트를 담을 풀
    private ObjectPool<T> pool;

    [Header("생성할 프리팹")]
    [SerializeField] private T data;

    private void Awake()
    {
        if(data != null && initCount > 0)
        {
            for(int i = 0; i < initCount; i++)
            {
                Get(data);
            }
        }
    }

    public T Get(T data)
    {
        if(pool == null)
        {
            pool = new ObjectPool<T>(
                createFunc: CreateObject(data),
                actionOnGet: ActivatePoolObject,
                actionOnRelease: DisablePoolObject,
                actionOnDestroy: DestroyPoolObject,
                collectionCheck: false,
                defaultCapacity: initSize,
                maxSize: maxSize
                );
        }

        return pool.Get();
    }

    public void Release(T data)
    {
        pool.Release(data);
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
