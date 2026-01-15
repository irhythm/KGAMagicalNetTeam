using UnityEngine;

public class CoolTimeIconPool : MonoBehaviour
{
    [SerializeField] GameObject[] icon;

    public void SetCoolTimeIcon(GameObject[] skillIcon)
    {
        icon = skillIcon;
    }

    public GameObject GetPlayerIcon()
    {
        foreach (var i in icon)
        {
            if(i.activeSelf)
                return i;
        }
        return null;
    }
}
