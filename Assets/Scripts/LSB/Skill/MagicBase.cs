using UnityEngine;

public abstract class MagicBase
{
    protected MagicDataSO data; 
    protected float currentCooldown;

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
        Debug.Log($"ÇöÀç Äð´Ù¿î: {currentCooldown}");
        return currentCooldown <= 0;
    }

    public void InitCooldown()
    {
        currentCooldown = data.cooldown;
    }

    public abstract void OnCast(Vector3 spawnPos, Vector3 direction, bool isLeftHand, int shooterID);
}