using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Timeline;

// 상호작용 종류
public enum InteractionType
{
    Assassinate,    // 암살

}

[Serializable]
public struct TransformOffset
{
    public string name;
    public bool isApply;
    public Vector3 position;
    public Vector3 rotation;
}

// 0번 인덱스는 상호작용 발행자 / 1번 이후는 상호작용 수신자
[CreateAssetMenu(fileName = "InteractionDataSO", menuName = "Scriptable Objects/InteractionDataSO")]
public class InteractionDataSO : ScriptableObject
{
    [field: SerializeField] public TimelineAsset timelineAsset { get; private set; }

    [field: SerializeField] public InteractionType type { get; private set; }

    [field: SerializeField] public bool isExecuterPivot { get; private set; }

    [field: SerializeField] public int receiverPivotIndex = 0;

    public TransformOffset offset_Executer;
    public TransformOffset offset_Camera;
    public List<TransformOffset> offset_Receiver = new List<TransformOffset>();
}
