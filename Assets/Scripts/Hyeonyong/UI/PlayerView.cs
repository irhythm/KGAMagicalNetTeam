using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] Image playerHp;
    [SerializeField] Image voiceImage;
    [SerializeField] GameObject[] icon;
    [SerializeField] GameObject[] iconOnHand;
    [SerializeField] float checkDuration = 0.1f;
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

    public void SetMagicInfoOnHand(GameObject[] magic)
    {
        iconOnHand = magic;
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

    //public void SetIconOnHand(InventoryData data, bool isLeft)
    //{
    //    int num = 0;
    //    if (isLeft)
    //        num = 0;
    //    else
    //        num = 1;
    //    iconOnHand[num].GetComponent<Image>().sprite = data.itemImage;
    //    MagicData magic = data as MagicData;
    //    if (magic != null)
    //    {
    //        iconOnHand[num].transform.GetChild(0).GetComponent<Image>().fillAmount = magic.cooldown;
    //    }
    //    else
    //    {
    //        iconOnHand[num].transform.GetChild(0).GetComponent<Image>().fillAmount = 0;
    //    }
    //}

    //public void SetMagicIcon(InventoryData data, Sprite magicIcon, float coolDown)
    //{
    //    MagicData magicData = data as MagicData;
    //    if (magicData != null)
    //    {
    //        GameObject magic = GetMagicIcon();
    //        magic.GetComponent<Image>().sprite = magicIcon;
    //        Image checkCoolDown = magic.transform.GetChild(0).GetComponent<Image>();
    //        checkCoolDown.fillAmount = 1;

    //        StartCoroutine(CheckCoolTime(checkCoolDown, coolDown, magic));
    //    }
    //}

    public IEnumerator CheckCoolTime(Image checkCoolDown, float coolDown, GameObject magicIcon=null)
    {
        float curCoolDown = coolDown;
        while (curCoolDown > 0)
        {
            if (curCoolDown > checkDuration)
                yield return CoroutineManager.waitForSeconds(checkDuration);
            else
                yield return CoroutineManager.waitForSeconds(curCoolDown);
            curCoolDown -= checkDuration;
            checkCoolDown.fillAmount = curCoolDown / coolDown;
        }
        if(magicIcon != null) 
            {
                magicIcon.SetActive(false);
            }
    }
}
