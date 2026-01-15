using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] Image playerHp;
    [SerializeField] Image voiceImage;
    [SerializeField] GameObject[] icon;
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

    public void SetMagicInfo(GameObject[] magic)
    {
        icon = magic;
    }

    public GameObject GetMagicIcon()
    {
        foreach (var i in icon)
        {
            if (i.activeSelf)
                return i;
        }
        return null;
    }

    public void SetMagicIcon(Sprite magicIcon, float coolDown)
    {
        GameObject magic = GetMagicIcon();
        magic.GetComponent<Image>().sprite = magicIcon;
        Image checkCoolDown = magic.transform.GetChild(0).GetComponent<Image>();
        checkCoolDown.fillAmount = 1;

        StartCoroutine(CheckCoolTime(magic, checkCoolDown,coolDown));


    }

    public IEnumerator CheckCoolTime(GameObject magic, Image checkCoolDown, float coolDown)
    {
        float curCoolDown = coolDown;
        while (curCoolDown > 0)
        {
            if (curCoolDown > 1f)
                yield return CoroutineManager.waitForSeconds(1f);
            else
                yield return CoroutineManager.waitForSeconds(curCoolDown);
            curCoolDown -= 1f;
            checkCoolDown.fillAmount = curCoolDown / coolDown;
        }
        magic.SetActive(false);
    }
}
