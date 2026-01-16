using UnityEngine;

public abstract class MagicBase
{
    protected MagicDataSO data;
    protected float currentCooldown;

    public MagicDataSO Data
    {
        get { return data; }
    }
    public float CurrentCooldown
    {
        get { return currentCooldown; }
    }

    public MagicBase(MagicDataSO data)
    {
        this.data = data;
    }

    public void Tick(float deltaTime)
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= deltaTime;

            if (currentCooldown <= 0) 
                currentCooldown = 0;
        }
    }

    public bool CanCast()
    {
        Debug.Log($"현재 쿨다운: {currentCooldown}");
        return currentCooldown <= 0;
    }

    public void InitCooldown()
    {
        currentCooldown = data.cooldown;
    }

    public abstract void OnCast(Vector3 spawnPos, Vector3 direction, bool isLeftHand, int shooterID);
}