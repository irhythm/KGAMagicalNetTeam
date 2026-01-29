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

    [Header("Visual 관련")]
    public GameObject VisualPrefab;
    public GameObject[] PolymorphPrefabs;

    [Header("Execution 시 변경될 애니메이터")]
    public RuntimeAnimatorController ExecutionAnimator;

    public DebuffInfo(DebuffType type, float duration, float value = 0f, 
        GameObject visual = null, GameObject[] polyPrefabs = null,
        RuntimeAnimatorController execAnim = null)
    {
        Type = type;
        Duration = duration;
        Value = value;
        VisualPrefab = visual;
        PolymorphPrefabs = polyPrefabs;
        ExecutionAnimator = execAnim;
    }
}

//AI, Player가 상속받아야 할 인터페이스

public interface IDebuffable
{
    GameObject gameObject { get; }
    Transform transform { get; }

    void SetSpeed(float speed);
    void SetCanMove(bool canMove);
    float GetOriginalSpeed();

    Animator GetAnimator();
    void SetModelVisibility(bool isVisible);
    Transform GetCenterPosition();
    void ApplyDebuff(DebuffInfo info);
    void RemoveDebuff(DebuffType type);
}



//디버프 전략이 구현할 인터페이스
public interface IDebuffBehavior
{
    void OnEnter(IDebuffable target, DebuffInfo info);
    void OnExecute(IDebuffable target);
    void OnExit(IDebuffable target);
}

