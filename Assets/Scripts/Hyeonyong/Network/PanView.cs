using UnityEngine;
using UnityEngine.UI;

public class PanView : MonoBehaviour
{
    [SerializeField] Image hpBar;
    public void UpdateHp(float ratio)
    {
        if (hpBar == null) return;
        hpBar.fillAmount = ratio;
    }
}
