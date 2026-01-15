using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Slots")]
    public ItemData LeftHandSlot;
    public ItemData RightHandSlot;

    [Header("Storage")]
    [SerializeField] private List<ItemData> inventory = new List<ItemData>();

    public IReadOnlyList<ItemData> Inventory => inventory;

    // 아이템 추가
    public void AddItem(ItemData item)
    {
        if (inventory.Contains(item)) return;
        if (inventory.Count >= 8)
        {
            Debug.LogWarning("인벤토리가 가득 찼습니다");
            return;
        }
        inventory.Add(item);
    }

    // 아이템 제거
    public void RemoveItem(ItemData item)
    {
        if (inventory.Contains(item))
            inventory.Remove(item);
    }

    // 왼손이면 true 오른손이면 false
    public void EquipItem(ItemData item, bool isLeft)
    {
        if (isLeft) LeftHandSlot = item;
        else RightHandSlot = item;

        Debug.Log($"{(isLeft ? "좌측(Q)" : "우측(E)")} 슬롯 장착: {item.itemName}");
    }
}