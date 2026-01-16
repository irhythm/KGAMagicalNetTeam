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
public abstract class ObjectPoolManager<T> : MonoBehaviour where T : Component
{
    // 오브젝트를 담을 풀
    public ObjectPool<T> pool { get; set; }

    [Header("생성할 프리팹")]
    [SerializeField] private T prefab;

    [Header("초기 생성 값")]
    [Range(1, 50)]
    [SerializeField] private int initSize = 4;

    [Header("최대 생성 값")]
    [Range(0, 200)]
    [SerializeField] private int maxSize = 200;

    private void Awake()
    {
        pool = new ObjectPool<T>(
            CreateObject, ActivatePoolObject, DisablePoolObject, DestroyPoolObject, false, initSize, maxSize);
    }

    // 생성할 때
    protected virtual T CreateObject()
    {
        return Instantiate(prefab, transform);
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
