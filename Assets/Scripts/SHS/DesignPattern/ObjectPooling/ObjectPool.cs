using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 데이터를 오브젝트 풀로 관리하는 클래스
/// 오브젝트 풀로 관리할 데이터가 하나의 종류만 있을 경우 사용
/// </summary>
/// <typeparam name="T"> 풀로 관리할 객체 타입 </typeparam>
public class ObjectPool<T> : Singleton<ObjectPool<T>> where T : MonoBehaviour
{
    //T타입 데이터를 담을 큐
    protected Queue<T> pool = new Queue<T>();

    protected override void Awake()
    {
        base.Awake();
    }

    public T GetObject(T type, Transform trans)
    {
        T data;

        if (pool.Count == 0)
        {
            data = Instantiate(type, trans);
            data.name = type.name;
        }
        else
        {
            data = pool.Dequeue();
            data.gameObject.SetActive(true);
        }

        return data;
    }

    // todo: Take할 때 SetParent를 풀 오브젝트 하위에 둘지 고려중
    public void TakeObject(T data)
    {
        if (data == null) return;

        data.transform.SetParent(transform, false);
        data.gameObject.SetActive(false);
        pool.Enqueue(data);
    }
}
