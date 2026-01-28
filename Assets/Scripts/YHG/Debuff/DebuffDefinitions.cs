using UnityEngine;

//디버프 관련 타입, 인터페이스 모아두는 cs

public enum DebuffType
{
    None,
    Stun,       //기절(감전?)
    Polymorph,  //변이, 자동이동?
    Slow,
    Execution   //처형(처형씬타임라인용)
}

//디버프 정보 구조체
[System.Serializable]
public struct DebuffInfo
{
    //종류, 지속, 밸류, 교체모델 or 이펙트 프리팹
    public DebuffType Type;
    public float Duration;
    public float Value;
    public GameObject VisualPrefab;

    public DebuffInfo(DebuffType type, float duration, float value = 0f, GameObject visual = null)
    {
        Type = type;
        Duration = duration;
        Value = value;
        VisualPrefab = visual;
    }
}
