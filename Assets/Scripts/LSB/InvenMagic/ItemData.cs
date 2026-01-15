using UnityEngine;

[CreateAssetMenu(fileName = "New ItemDate", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    public string id;           // 아이디
    public string itemName;     // 이름
    public Sprite icon;         // 이미지
    public GameObject prefab;   // 프리펩

    [TextArea]
    public string description;  // 설명
}