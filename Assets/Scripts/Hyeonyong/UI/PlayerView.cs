using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] Image playerHp;

    public void SetPlayerInfo(TextMeshProUGUI name, Image hp)
    {
        playerName=name;
        playerHp = hp;

    }

    public void SetPlayerName(string name)
    {
        playerName.text = name;
    }
    public void UpdatePlayerHP(float amount)
    {
        playerHp.fillAmount = amount;
    }
}
