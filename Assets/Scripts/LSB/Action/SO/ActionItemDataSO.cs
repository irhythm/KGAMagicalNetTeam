using UnityEngine;

public abstract class ActionItemDataSO : InventoryDataSO
{
    [Header("Action Common")]
    public float cooldown;
    public bool isDisposable;

    public abstract ActionBase CreateActionInstance();
}