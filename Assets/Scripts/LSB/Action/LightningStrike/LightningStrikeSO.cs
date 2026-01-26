using UnityEngine;

[CreateAssetMenu(fileName = "New LightningStrike", menuName = "Game/LightningStrike")]
public class LightningStrikeSO : MagicDataSO
{
    [Header("Strike Settings")]
    public float strikeRadius = 5f;       // 타격 범위
    public float strikeDelay = 0.5f;      // 마법 시전 후 데미지가 들어가기까지의 시간

    [Header("Visuals")]
    public GameObject strikeEffectPrefab; // 실제 쾅 하고 떨어지는 번개 이펙트
    public AudioClip lightningSound;
    public LayerMask hitLayer;            // 맞을 레이어

    public override ActionBase CreateInstance()
    {
        return new MagicLightningStrike(this);
    }
}
