using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory
{
    const int maximumInvenCount = 8;

    private Dictionary<InventoryDataSO, int> inventory = new Dictionary<InventoryDataSO, int>();

    private Dictionary<ActionItemDataSO, ActionBase> activeActions = new Dictionary<ActionItemDataSO, ActionBase>();

    public IReadOnlyDictionary<InventoryDataSO, int> Inventory => inventory;

    public void HandleCooldowns(float deltaTime)
    {
        foreach (var action in activeActions.Values)
        {
            action.Tick(deltaTime);
        }
    }

    public ActionBase GetActionInstance(ActionItemDataSO data)
    {
        if (data == null) return null;

        if (activeActions.TryGetValue(data, out ActionBase instance))
        {
            return instance;
        }
        return null;
    }

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

            if (item is ActionItemDataSO actionData)
            {
                if (!activeActions.ContainsKey(actionData))
                {
                    ActionBase newAction = actionData.CreateInstance();
                    activeActions.Add(actionData, newAction);
                    Debug.Log($"인벤토리 {item.itemName} 액션 생성됨");
                }
            }
        }

        if (GameManager.Instance != null && GameManager.Instance.InventoryWheel != null)
            GameManager.Instance.InventoryWheel.UpdateWheelInventory();
    }

    public void RemoveItem(InventoryDataSO item)
    {
        if (inventory.ContainsKey(item))
        {
            inventory[item]--;
            if (inventory[item] <= 0)
            {
                inventory.Remove(item);

                if (item is ActionItemDataSO actionData && activeActions.ContainsKey(actionData))
                {
                    activeActions.Remove(actionData);
                }
                return;
            }
        }
        if (GameManager.Instance != null && GameManager.Instance.InventoryWheel != null)
            GameManager.Instance.InventoryWheel.UpdateWheelInventory();
    }
}