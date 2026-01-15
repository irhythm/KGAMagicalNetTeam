using UnityEngine;

[CreateAssetMenu(fileName = "New MagicDate", menuName = "Game/MagicData")]
public class MagicData : InventoryData
{
    public bool isDisposable;  // 일회용인지
    public int magicDamage;    // 마법 데미지
    public float cooldown;     // 쿨타운
}