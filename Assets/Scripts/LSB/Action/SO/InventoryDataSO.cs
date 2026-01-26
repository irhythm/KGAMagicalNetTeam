using UnityEngine;

public class InventoryDataSO : ScriptableObject
{
    public string itemId;           // 아이디
    public string itemName;         // 이름

    public Sprite itemImage;        // 이미지
    
    public GameObject itemPrefab;   // 프리펩

    [TextArea]
    public string description;      // 설명
}