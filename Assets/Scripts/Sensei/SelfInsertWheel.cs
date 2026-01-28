using UnityEngine;
using System.Collections;
public class SelfInsertWheel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {

        yield return new WaitUntil(() =>
            GameManager.Instance != null &&
            GameManager.Instance.InventoryWheel != null);

        // UI 할당
        GameManager.Instance.InventoryWheel.InventoryUI = gameObject;
        GameManager.Instance.InventoryWheel.UpdateWheelInventory();

        Debug.Log($"{gameObject.name}이 인벤토리 UI로 등록되었습니다.");
    }

    
}
