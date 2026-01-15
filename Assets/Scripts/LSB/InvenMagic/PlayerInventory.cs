using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory
{
    const int maximumInvenCount = 8;

    private Dictionary<InventoryData, int> inventory = new Dictionary<InventoryData, int>();

    public IReadOnlyDictionary<InventoryData, int> Inventory => inventory;

    // 아이템 추가
    public void AddItem(InventoryData item)
    {
        if (inventory.ContainsKey(item))
        {
            inventory[item]++;
            return;
        }
        if (inventory.Count >= maximumInvenCount)
        {
            Debug.Log("인벤토리 가득 참");
            return;
        }
        inventory.Add(item, 1);
    }

    // 아이템 제거
    public void RemoveItem(InventoryData item)
    {
        if (inventory.ContainsKey(item))
        {
            inventory[item]--;
            if (inventory[item] <= 0)
            {
                inventory.Remove(item);
                return;
            }
            return;
        }
    }
}