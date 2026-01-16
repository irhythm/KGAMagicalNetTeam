using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory
{
    const int maximumInvenCount = 8;

    private Dictionary<InventoryDataSO, int> inventory = new Dictionary<InventoryDataSO, int>();

    private Dictionary<MagicDataSO, MagicBase> activeMagics = new Dictionary<MagicDataSO, MagicBase>();

    public IReadOnlyDictionary<InventoryDataSO, int> Inventory => inventory;


    
    


    public void HandleCooldowns(float deltaTime)
    {
        foreach (var magic in activeMagics.Values)
        {
            magic.Tick(deltaTime);
        }
    }

    public MagicBase GetMagicInstance(MagicDataSO data)
    {
        if (data == null) return null;

        if (activeMagics.TryGetValue(data, out MagicBase instance))
        {
            return instance;
        }
        return null;
    }

    // 아이템 추가
    public void AddItem(InventoryDataSO item)
    {
        if (inventory.ContainsKey(item))
        {
            inventory[item]++;
            
        }
        else
        {
            if (inventory.Count >= maximumInvenCount)
            {
                Debug.Log("인벤토리 가득 참");
                return;
            }
            inventory.Add(item, 1);

            if (item is MagicDataSO magicData)
            {
                if (!activeMagics.ContainsKey(magicData))
                {
                    MagicBase newMagic = magicData.CreateInstance();
                    activeMagics.Add(magicData, newMagic);
                    Debug.Log($"인벤토리 {item.itemName} 생성됨");
                }
            }
        }
        //260116 최정욱 intentorywheel 관련추가
        GameManager.Instance.InventoryWheel.UpdateWheelInventory();
    }

    // 아이템 제거
    public void RemoveItem(InventoryDataSO item)
    {
        if (inventory.ContainsKey(item))
        {
            inventory[item]--;
            if (inventory[item] <= 0)
            {
                inventory.Remove(item);

                if (item is MagicDataSO magicData && activeMagics.ContainsKey(magicData))
                {
                    activeMagics.Remove(magicData);
                }
                return;
            }
        }
        //260116 최정욱 intentorywheel 관련추가
        GameManager.Instance.InventoryWheel.UpdateWheelInventory();
    }
}