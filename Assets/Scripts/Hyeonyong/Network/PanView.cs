using UnityEngine;
using UnityEngine.UI;

public class PanView : MonoBehaviour
{
    [SerializeField] Image hpBar;
    public void UpdateHp(float ratio)
    { 
        hpBar.fillAmount = ratio;
    }
}
