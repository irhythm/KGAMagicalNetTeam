using UnityEditor.Overlays;
using UnityEngine;

public abstract class ItemAction : ActionBase
{
    // 데이터 형변환 프로퍼티
    public ItemDataSO MagicData => BaseData as ItemDataSO;

    public ItemAction(MagicDataSO data) : base(data)
    {
    }

    public abstract void OnUse(Vector3 spawnPos, Vector3 direction, bool isLeftHand, int shooterID);
}
