using UnityEngine;

public abstract class ActionItemDataSO : InventoryDataSO
{
    [Header("Action Setting")]
    public float cooldown;
    public bool isDisposable;

    public abstract ActionBase CreateInstance();
}