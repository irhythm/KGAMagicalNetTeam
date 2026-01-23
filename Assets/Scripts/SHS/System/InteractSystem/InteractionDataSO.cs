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

    [field: SerializeField] public List<string> trackNames { get; private set; }

    [field: SerializeField] public bool isExecuterPivot { get; private set; }

    [field: SerializeField] public int receiverPivotIndex = 0;

    public List<TransformOffset> offsets = new List<TransformOffset>();

    private void OnValidate()
    {
        int count = trackNames.Count;

        // trackNames 개수만큼 offsets 개수 맞추기
        if (trackNames.Count != offsets.Count)
        {
            int gap = Mathf.Abs(offsets.Count - trackNames.Count);

            if(trackNames.Count < offsets.Count)
            {
                offsets.RemoveRange(trackNames.Count, gap);
            }
            else
            {
                for(int i = 0; i < gap; i++)
                {
                    offsets.Add(new TransformOffset());
                }
            }
        }

        // isExecuterPivot에 따라 기준이 될 offsets 인덱스 맞추기
        if (!isExecuterPivot)
        {
            if(receiverPivotIndex == 0 || receiverPivotIndex + 1 > offsets.Count)
            {
                receiverPivotIndex = 1;
            }
        }

        // isExcuterPivot에 따라 isApply(오프셋 적용 여부) 꺼주기
        TransformOffset copy;

        if(isExecuterPivot)
        {
            if (offsets.Count >= 1 && offsets[0].isApply != false)
            {
                copy = offsets[0];
                copy.isApply = false;
                offsets[0] = copy;
            }
        }
        else
        {
            if (offsets.Count >= receiverPivotIndex + 1 && offsets[receiverPivotIndex].isApply != false)
            {
                copy = offsets[receiverPivotIndex];
                copy.isApply = false;
                offsets[receiverPivotIndex] = copy;
            }
        }
    }
}
