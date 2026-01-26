using UnityEngine;
using Photon.Pun;

public abstract class ActionBase
{
    // 데이터 참조
    public ActionItemDataSO BaseData { get; private set; }

    // 쿨타임 관리
    public float CurrentCooldown { get; protected set; }

    public ActionBase(ActionItemDataSO data)
    {
        this.BaseData = data;
    }

    // 쿨타임 감소 로직 업데이트에서 호출 해야됨
    public void Tick(float deltaTime)
    {
        if (CurrentCooldown > 0)
        {
            CurrentCooldown -= deltaTime;
            if (CurrentCooldown <= 0) CurrentCooldown = 0;
        }
    }

    // 사용 가능 여부
    public bool CanUse()
    {
        return CurrentCooldown <= 0;
    }

    // 쿨타임 시작
    public void InitCooldown()
    {
        CurrentCooldown = BaseData.cooldown;
    }
}