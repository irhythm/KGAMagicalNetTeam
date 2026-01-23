using UnityEngine;

public class MapIcon : MonoBehaviour
{
    public MapIconType type;

    private GameObject iconObj;

    private void OnEnable()
    {
        if (type == null || type.Icon == null) return;

        if(iconObj == null)
        {
            iconObj = new GameObject(type.name);
            iconObj.layer = LayerMask.NameToLayer("Minimap");
            iconObj.transform.SetParent(transform);
            iconObj.transform.localPosition = new Vector3(0, 10, 0);
            iconObj.transform.rotation = Quaternion.Euler(90, 0, 0);
            iconObj.AddComponent<SpriteRenderer>().sprite = type.Icon;
        }
        else
        {
            iconObj.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (type == null || type.Icon == null) return;
        if (iconObj == null) return;

        iconObj.SetActive(false);
    }
}
