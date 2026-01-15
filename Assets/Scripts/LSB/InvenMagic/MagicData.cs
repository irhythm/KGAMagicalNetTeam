using UnityEngine;

public abstract class MagicData : InventoryData
{
    public bool isDisposable;   // 일회용인지
    public int magicDamage;     // 마법 데미지
    public float cooldown;      // 쿨타운
    public Vector3 spawnOffset; // 생성 위치 오프셋

    public abstract void OnCast(Vector3 spawnPos, Vector3 direction, bool isLeftHand);
}