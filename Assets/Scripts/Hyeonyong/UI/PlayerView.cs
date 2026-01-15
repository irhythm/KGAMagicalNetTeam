using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] Image playerHp;
    [SerializeField] Image voiceImage;

    public void SetVoiceImage(Image photonVoiceImage)
    {
        voiceImage= photonVoiceImage;
        voiceImage.enabled= false;
    }

    public void CheckVoiceImage(bool check)
    {
        voiceImage.enabled = check;
    }

    public void SetPlayerInfo(TextMeshProUGUI name, Image hp, Image voice)
    {
        playerName=name;
        playerHp = hp;
        voiceImage= voice;
    }
    public void SetMyInfo(TextMeshProUGUI name, Image hp)
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

    public void OnVoiceImage()
    {
        
    }
}
