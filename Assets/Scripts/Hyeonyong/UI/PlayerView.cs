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

    Coroutine leftHandIconCoolTimeCoroutine;
    Coroutine rightHandIconCoolTimeCoroutine;

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
        leftHandIcon = magic[0].transform.GetChild(0).GetComponent<Image>();
        rightHandIcon = magic[1].transform.GetChild(0).GetComponent<Image>();

        leftHandIconCoolTime = magic[0].transform.GetChild(0).GetChild(0).GetComponent<Image>();
        rightHandIconCoolTime = magic[1].transform.GetChild(0).GetChild(0).GetComponent<Image>();
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

    private void HandleInventoryCooldownCheck(InventoryDataSO data)
    {
        if (data == null || _player == null) return;

        ActionItemDataSO actionData = data as ActionItemDataSO;
        if (actionData == null) return;

        ActionBase targetLogic = _player.Inventory.GetActionInstance(actionData);

        if (targetLogic != null && targetLogic.CurrentCooldown > 0)
        {
            SetMagicIcon(targetLogic);
        }
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

    public void SetIconOnHand(InventoryDataSO data, bool isLeft)
    {
        if (isLeft && leftHandIconCoolTimeCoroutine != null)
        {
            StopCoroutine(leftHandIconCoolTimeCoroutine);
        }
        else if (!isLeft && rightHandIconCoolTimeCoroutine != null)
        {
            StopCoroutine(rightHandIconCoolTimeCoroutine);
        }

        // 260116 손 비우기 용 코드 최정욱
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

    public void CheckCoolTimeOnHand(ActionBase action, bool isLeft)
    {
        if (action == null) return;

        float curCoolTime = action.CurrentCooldown;
        float maxCoolTime = action.BaseData.cooldown;

        Image coolTimeIcon = null;
        if (isLeft)
            coolTimeIcon = leftHandIconCoolTime;
        else
            coolTimeIcon = rightHandIconCoolTime;

        coolTimeIcon.fillAmount = curCoolTime / maxCoolTime;

        if (isLeft)
            leftHandIconCoolTimeCoroutine = StartCoroutine(CheckCoolTimeOnHand(coolTimeIcon, curCoolTime, maxCoolTime));
        else
            rightHandIconCoolTimeCoroutine = StartCoroutine(CheckCoolTimeOnHand(coolTimeIcon, curCoolTime, maxCoolTime));
    }

    public void SetMagicIcon(ActionBase action)
    {
        GameObject magicIcon = GetMagicIcon();
        if (magicIcon == null) return;

        magicIcon.transform.GetChild(0).GetComponent<Image>().sprite = action.BaseData.itemImage;
        Image coolTimeImage = magicIcon.transform.GetChild(0).GetChild(0).GetComponent<Image>();

        float curCoolTime = action.CurrentCooldown;
        float maxCoolTime = action.BaseData.cooldown;
        coolTimeImage.fillAmount = curCoolTime / maxCoolTime;
        magicIcon.SetActive(true);
        StartCoroutine(CheckCoolTimeOnInventory(coolTimeImage, curCoolTime, maxCoolTime, magicIcon));
    }

    public IEnumerator CheckCoolTimeOnHand(Image checkCoolDown, float curCoolTime, float maxCoolTime)
    {
        while (curCoolTime > 0)
        {
            if (curCoolTime > checkDuration)
                yield return CoroutineManager.waitForSeconds(checkDuration);
            else
                yield return CoroutineManager.waitForSeconds(curCoolTime);
            curCoolTime -= checkDuration;
            checkCoolDown.fillAmount = curCoolTime / maxCoolTime;
        }
    }

    public IEnumerator CheckCoolTimeOnInventory(Image checkCoolDown, float curCoolTime, float maxCoolTime, GameObject magicIcon)
    {
        while (curCoolTime > 0)
        {
            if (curCoolTime > checkDuration)
                yield return CoroutineManager.waitForSeconds(checkDuration);
            else
                yield return CoroutineManager.waitForSeconds(curCoolTime);
            curCoolTime -= checkDuration;
            checkCoolDown.fillAmount = curCoolTime / maxCoolTime;
        }
        if (magicIcon != null)
        {
            magicIcon.SetActive(false);
        }
    }
}