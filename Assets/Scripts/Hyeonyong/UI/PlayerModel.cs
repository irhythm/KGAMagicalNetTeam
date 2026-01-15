using UnityEngine;

public class PlayerModel
{
    private float maxHp = 100;
    public float MaxHp => maxHp;

    private float curHp;
    public float CurHp => curHp;

    public PlayerModel(float maxHp)
    {
        this.maxHp = maxHp;
        curHp = maxHp;
    }

    public void Init()
    {
        curHp = maxHp;
    }

    public bool TakeDamage(float takeDamage)
    {
        //방어력으로 인한 피격 데미지 감소(2차)
        curHp -= takeDamage;
        if (curHp <= 0)
        {
            curHp = 0;
            return true;
        }
        return false;
    }
}