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
    [SerializeField] GameObject[] iconOnHand;
    [SerializeField] float checkDuration = 0.1f;
    Image leftHandIcon;
    Image leftHandIconCoolTime;
    Image rightHandIcon;
    Image rightHandIconCoolTime;

    private PlayerMagicSystem _boundSystem;
    private PlayableCharacter _player;

    public void SetVoiceImage(Image photonVoiceImage)
    {
        voiceImage = photonVoiceImage; 
        voiceImage.enabled = false;
    }

    public void CheckVoiceImage(bool check)
    {
        voiceImage.enabled = check;
    }

    public void SetPlayerInfo(TextMeshProUGUI name, Image hp, Image voice)
    {
        playerName = name; 
        playerHp = hp; 
        voiceImage = voice;
    }

    public void SetMyInfo(TextMeshProUGUI name, Image hp)
    {
        playerName = name; 
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
        leftHandIcon = magic[0].GetComponent<Image>();
        rightHandIcon = magic[1].GetComponent<Image>();

        leftHandIconCoolTime = magic[0].transform.GetChild(0).GetComponent<Image>();
        rightHandIconCoolTime = magic[1].transform.GetChild(0).GetComponent<Image>();
    }

    public void BindMagicSystem(PlayerMagicSystem system, PlayableCharacter player)
    {
        if (_boundSystem != null) 
            UnsubscribeEvents();

        _boundSystem = system;
        _player = player;

        if (_boundSystem != null) 
            SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (_boundSystem == null) return;
        _boundSystem.OnHandItemChanged += SetIconOnHand;
        _boundSystem.OnHandCooldownStarted += CheckCoolTimeOnHand;
        _boundSystem.OnInventoryCooldownCheck += HandleInventoryCooldownCheck;
    }

    private void UnsubscribeEvents()
    {
        if (_boundSystem == null) return;
        _boundSystem.OnHandItemChanged -= SetIconOnHand;
        _boundSystem.OnHandCooldownStarted -= CheckCoolTimeOnHand;
        _boundSystem.OnInventoryCooldownCheck -= HandleInventoryCooldownCheck;
    }

    // 인벤토리 아이템 쿨타임 확인용
    private void HandleInventoryCooldownCheck(InventoryDataSO data)
    {
        if (data == null || _player == null) return;

        MagicDataSO magicData = data as MagicDataSO;
        if (magicData == null) return;

        MagicBase targetLogic = _player.Inventory.GetMagicInstance(magicData);

        if (targetLogic != null && targetLogic.CurrentCooldown > 0)
        {
            SetMagicIcon(targetLogic);
        }
    }

    public void SetIconOnHand(InventoryDataSO data, bool isLeft)
    {
        if (data == null)
        {
            if (isLeft) 
            { 
                leftHandIcon.sprite = null; 
                leftHandIconCoolTime.fillAmount = 0; 
            }
            else 
            { 
                rightHandIcon.sprite = null; 
                rightHandIconCoolTime.fillAmount = 0; 
            }
            return;
        }

        if (data.itemImage == null) 
            return;

        if (isLeft) 
        { 
            leftHandIcon.sprite = data.itemImage; 
            leftHandIconCoolTime.fillAmount = 0; 
        }
        else 
        { 
            rightHandIcon.sprite = data.itemImage; 
            rightHandIconCoolTime.fillAmount = 0; 
        }
    }

    public void CheckCoolTimeOnHand(MagicBase magic, bool isLeft)
    {
        if (magic == null) return;

        float curCoolTime = magic.CurrentCooldown;
        float maxCoolTime = magic.Data.cooldown;

        Image coolTimeIcon = isLeft ? leftHandIconCoolTime : rightHandIconCoolTime;

        coolTimeIcon.fillAmount = curCoolTime / maxCoolTime;
        StartCoroutine(CheckCoolTimeOnHand(coolTimeIcon, curCoolTime, maxCoolTime));
    }

    public GameObject GetMagicIcon()
    {
        foreach (var i in icon)
        {
            if (!i.activeSelf) 
                return i;
        }
        return null;
    }

    public void SetMagicIcon(MagicBase magic)
    {
        GameObject magicIcon = GetMagicIcon();
        if (magicIcon == null) 
            return;

        magicIcon.GetComponent<Image>().sprite = magic.Data.itemImage;
        Image coolTimeImage = magicIcon.transform.GetChild(0).GetComponent<Image>();

        float curCoolTime = magic.CurrentCooldown;
        float maxCoolTime = magic.Data.cooldown;
        coolTimeImage.fillAmount = curCoolTime / maxCoolTime;
        magicIcon.SetActive(true);
        StartCoroutine(CheckCoolTimeOnInventory(coolTimeImage, curCoolTime, maxCoolTime, magicIcon));
    }

    public IEnumerator CheckCoolTimeOnHand(Image checkCoolDown, float curCoolTime, float maxCoolTime)
    {
        while (curCoolTime > 0)
        {
            float waitTime = (curCoolTime > checkDuration) ? checkDuration : curCoolTime;
            yield return CoroutineManager.waitForSeconds(waitTime);

            curCoolTime -= waitTime;
            checkCoolDown.fillAmount = curCoolTime / maxCoolTime;
        }
    }

    public IEnumerator CheckCoolTimeOnInventory(Image checkCoolDown, float curCoolTime, float maxCoolTime, GameObject magicIcon)
    {
        while (curCoolTime > 0)
        {
            float waitTime = (curCoolTime > checkDuration) ? checkDuration : curCoolTime;
            yield return CoroutineManager.waitForSeconds(waitTime);

            curCoolTime -= waitTime;
            checkCoolDown.fillAmount = curCoolTime / maxCoolTime;
        }
        if (magicIcon != null)
        {
            magicIcon.SetActive(false);
        }
    }
}

#region 레거시 코드
//using System.Collections;
//using TMPro;
//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.UI;

//public class PlayerView : MonoBehaviour
//{
//    [SerializeField] TextMeshProUGUI playerName;
//    [SerializeField] Image playerHp;
//    [SerializeField] Image voiceImage;
//    [SerializeField] GameObject[] icon;
//    [SerializeField] GameObject[] iconOnHand;
//    [SerializeField] float checkDuration = 0.1f;
//    Image leftHandIcon;
//    Image leftHandIconCoolTime;
//    Image rightHandIcon;
//    Image rightHandIconCoolTime;
//    public void SetVoiceImage(Image photonVoiceImage)
//    {
//        voiceImage = photonVoiceImage;
//        voiceImage.enabled = false;
//    }

//    public void CheckVoiceImage(bool check)
//    {
//        voiceImage.enabled = check;
//    }

//    public void SetPlayerInfo(TextMeshProUGUI name, Image hp, Image voice)
//    {
//        playerName = name;
//        playerHp = hp;
//        voiceImage = voice;
//    }
//    public void SetMyInfo(TextMeshProUGUI name, Image hp)
//    {
//        playerName = name;
//        playerHp = hp;
//    }

//    public void SetPlayerName(string name)
//    {
//        playerName.text = name;
//    }
//    public void UpdatePlayerHP(float amount)
//    {
//        playerHp.fillAmount = amount;
//    }

//    public void SetMagicInfo(GameObject[] magic)
//    {
//        icon = magic;
//    }

//    public void SetMagicInfoOnHand(GameObject[] magic)
//    {
//        iconOnHand = magic;
//        leftHandIcon = magic[0].GetComponent<Image>();
//        rightHandIcon = magic[1].GetComponent<Image>();

//        leftHandIconCoolTime = magic[0].transform.GetChild(0).GetComponent<Image>();
//        rightHandIconCoolTime = magic[1].transform.GetChild(0).GetComponent<Image>();
//    }

//    public GameObject GetMagicIcon()
//    {
//        foreach (var i in icon)
//        {
//            if (!i.activeSelf)
//                return i;
//        }
//        return null;
//    }

//    public void SetIconOnHand(InventoryDataSO data, bool isLeft)
//    {
//        //260116 손 비우기 용 코드 최정욱
//        if (data == null)
//        {
//            if (isLeft)
//            {
//                leftHandIcon.sprite = null;
//                leftHandIconCoolTime.fillAmount = 0;
//            }
//            else
//            {
//                rightHandIcon.sprite = null;
//                rightHandIconCoolTime.fillAmount = 0;
//            }
//            return;
//        }

//        if (data.itemImage == null)
//            return;
//        if (isLeft)
//        {
//            leftHandIcon.sprite = data.itemImage;
//            leftHandIconCoolTime.fillAmount = 0;
//        }
//        else
//        {
//            rightHandIcon.sprite = data.itemImage;
//            rightHandIconCoolTime.fillAmount = 0;
//        }
//            //iconOnHand[num].GetComponent<Image>().sprite = data.itemImage;

//    }
//    public void CheckCoolTimeOnHand(MagicBase magic, bool isLeft)
//    {
//        float curCoolTime = magic.CurrentCooldown;
//        float maxCoolTime = magic.Data.cooldown;

//        Image coolTimeIcon = null;
//        if (isLeft)
//            coolTimeIcon = leftHandIconCoolTime;
//        else
//            coolTimeIcon = rightHandIconCoolTime;

//        coolTimeIcon.fillAmount = curCoolTime / maxCoolTime;
//        StartCoroutine(CheckCoolTimeOnHand(coolTimeIcon, curCoolTime, maxCoolTime));
//    }


//    public void SetMagicIcon(MagicBase magic)
//    {
//        GameObject magicIcon = GetMagicIcon();
//        magicIcon.GetComponent<Image>().sprite = magic.Data.itemImage;
//        Image coolTimeImage = magicIcon.transform.GetChild(0).GetComponent<Image>();

//        float curCoolTime = magic.CurrentCooldown;
//        float maxCoolTime = magic.Data.cooldown;
//        coolTimeImage.fillAmount = curCoolTime / maxCoolTime;
//        magicIcon.SetActive(true);
//        StartCoroutine(CheckCoolTimeOnInventory(coolTimeImage,curCoolTime,maxCoolTime, magicIcon));        
//    }



//    public IEnumerator CheckCoolTimeOnHand(Image checkCoolDown, float curCoolTime, float maxCoolTime)
//    {
//        while (curCoolTime > 0)
//        {
//            if (curCoolTime > checkDuration)
//                yield return CoroutineManager.waitForSeconds(checkDuration);
//            else
//                yield return CoroutineManager.waitForSeconds(curCoolTime);
//            curCoolTime -= checkDuration;
//            checkCoolDown.fillAmount = curCoolTime / maxCoolTime;
//        }
//    }

//    public IEnumerator CheckCoolTimeOnInventory(Image checkCoolDown, float curCoolTime, float maxCoolTime, GameObject magicIcon)
//    {
//        while (curCoolTime > 0)
//        {
//            if (curCoolTime > checkDuration)
//                yield return CoroutineManager.waitForSeconds(checkDuration);
//            else
//                yield return CoroutineManager.waitForSeconds(curCoolTime);
//            curCoolTime -= checkDuration;
//            checkCoolDown.fillAmount = curCoolTime / maxCoolTime;
//        }
//        if (magicIcon != null)
//        {
//            magicIcon.SetActive(false);
//        }
//    }
//}
#endregion