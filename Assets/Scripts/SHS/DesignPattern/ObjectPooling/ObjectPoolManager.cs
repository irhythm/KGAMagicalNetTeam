using System;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [Header("초기 관리 풀 크기")]
    [Range(1, 50)]
    [SerializeField] protected int initSize = 4;        // 풀의 처음 크기

    [Header("최대 관리 풀 크기")]
    [Range(0, 200)]
    [SerializeField] protected int maxSize = 200;       // 풀이 최대 몇개를 관리할지

    [Header("초기 생성 개수")]
    [Range(0, 200)]
    [SerializeField] protected int initCount = 0;       // 처음에 미리 생성해둘 개수

    protected void OnValidate()
    {
        if (initCount > maxSize)
            initCount = maxSize;
    }
}
