using UnityEngine;

public class ItemCoin : ItemAction
{
    public ItemCoin(ItemDataSO data) : base(data)
    {
    }

    public override void OnUse(Vector3 spawnPos, Vector3 direction, bool isLeftHand, int shooterID)
    {
        throw new System.NotImplementedException();
    }
}
