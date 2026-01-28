using UnityEngine;

[System.Serializable]
public class DebuffInfo : MonoBehaviour
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
