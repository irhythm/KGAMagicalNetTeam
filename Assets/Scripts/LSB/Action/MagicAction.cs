using UnityEngine;

public abstract class MagicAction : ActionBase
{
    // 데이터 형변환 프로퍼티
    public MagicDataSO MagicData => BaseData as MagicDataSO;

    public MagicAction(MagicDataSO data) : base(data)
    {
    }

    public abstract void OnCast(Vector3 spawnPos, Vector3 targetPos, bool isLeftHand, int shooterID);
}