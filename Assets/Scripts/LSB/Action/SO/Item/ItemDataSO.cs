using UnityEngine;
public abstract class ItemDataSO : ActionItemDataSO
{
    [Header("Item Settings")]
    [Min(1)] public int maxStackSize = int.MaxValue;



    public abstract override ActionBase CreateInstance();
}