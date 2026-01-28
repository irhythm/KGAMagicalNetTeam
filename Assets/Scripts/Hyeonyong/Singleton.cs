using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
                Debug.Log("No Singleton of this type exists: " + typeof(T).ToString());
                _instance = FindObjectOfType<T>();
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
                //if (_instance == null)
                //{
                    //GameObject singletonObject = new GameObject();
                    //_instance = singletonObject.AddComponent<T>();
                    //singletonObject.name = typeof(T).ToString() + " (Singleton)";
                    //DontDestroyOnLoad(singletonObject);
                //}
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (_instance != this)

            {
                Destroy(gameObject);
            }

        }
    }
}