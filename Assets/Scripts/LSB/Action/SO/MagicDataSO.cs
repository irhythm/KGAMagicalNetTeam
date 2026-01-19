using UnityEngine;

public abstract class MagicDataSO : InventoryDataSO
{
    public int damage;          // 데미지
    public bool isDisposable;   // 일회용인지
    public float cooldown;      // 쿨타운
    public Vector3 spawnOffset; // 생성 위치 오프셋
    public float knockbackForce;// 넉백 힘

    public abstract MagicBase CreateInstance();
}