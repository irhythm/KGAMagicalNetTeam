using NUnit.Framework;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
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

    //List<ActionBase> _actionCooltimeList =new List<ActionBase>();
    Dictionary<ActionBase, GameObject> _actionCooltimeDictionary = new Dictionary<ActionBase, GameObject>();
    Dictionary<ActionBase, Coroutine> _actionCooltimeCoroutineDictionary = new Dictionary<ActionBase, Coroutine>();
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

        //260119 최정욱
        //if (PlayerManager.LocalPlayerInstance.GetComponent<PlayableCharacter>().MagicSystem.LeftHandSlot == null)
        //{
        //    leftHandIcon.color = new Color(1, 1, 1, 0);
        //}
        //else
        //{
        //    leftHandIcon.color = new Color(1, 1, 1, 1);
        //}
        //if (PlayerManager.LocalPlayerInstance.GetComponent<PlayableCharacter>().MagicSystem.RightHandSlot == null)
        //{
        //    rightHandIcon.color = new Color(1, 1, 1, 0);
        //}
        //else
        //{
        //    rightHandIcon.color = new Color(1, 1, 1, 1);
        //}

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

    //data는 변경하고자 하는 손에 가지고 있던 데이터
    //newData는 장착하고자 하는 데이터
    //다시 손에 가져올 경우 리스트에서 해당 아이콘을 지운다
    private void HandleInventoryCooldownCheck(InventoryDataSO data, InventoryDataSO newData)
    {
        if (data == null || _player == null) return;
        if (newData == null)
            return;

        ActionItemDataSO actionData = data as ActionItemDataSO;
        ActionItemDataSO newActionData = newData as ActionItemDataSO;
        if (actionData == null) return;
        ActionBase targetLogic = _player.Inventory.GetActionInstance(actionData);
        if(targetLogic.CurrentCooldown<=0) return;

        ActionItemDataSO leftHandMagicData = _boundSystem.LeftHandSlot as ActionItemDataSO;
        ActionItemDataSO rightHandMagicData = _boundSystem.RightHandSlot as ActionItemDataSO;

        
        ActionBase LeftLogic = _player.Inventory.GetActionInstance(leftHandMagicData);
        ActionBase rightLogic = _player.Inventory.GetActionInstance(rightHandMagicData);

        if (newActionData != null)
        {
            ActionBase newLogic = _player.Inventory.GetActionInstance(newActionData);

            //바꾸고자 하는 데이터가 오른손 혹은 왼손에 있던 데이터일 경우
            //이거 문제점 발견
            //쿨타임 있는 마법을 반댓손에 넣는 거 막는 건 이해함
            //하지만 쿨타임이 아닌 마법을 쿨타임이 있는 쪽의 마법에 막을 경우 그것도 막아버림
            //그렇다면 들어가는 마법의 쿨타임이 있다면? 막지 않는다
            //다시 문제점 반댓 손에 쿨타임이 있는 마법을 넣었는데 이게 아이콘이 생성된다
            //인벤에 있는걸 꺼낼 때는 아이콘을 지우지만 손에 있는 걸옮길 때는 지우지 않는다
            //newLogic은 새로 들어갈 마법 targetLogic은 들어갈 위치에 있는 마법
            //이거 문제점이 해당 마법이 사라지면서 거기서 이 코드를 호출하는 것 같다
            //즉 불 마법 -> null 반댓손은 ??? ->불마법 이런식으로 바뀌어서 2 번 호출됨
            //그냥 SetIconOnHand에서 이걸 없애자
            if (newLogic == LeftLogic || newLogic == rightLogic)
            {
                if (targetLogic.CurrentCooldown <= 0)
                {
                    return;
                }
            }
        }

        //바꾸고자 하는 데이터가 이미 컨테인에 추가되어 있을 경우
        //if (_actionCooltimeList.Contains(targetLogic))
        //{
        //    Debug.Log("값이 들어 있다");
        //    return;
        //}
        if (_actionCooltimeDictionary.ContainsKey(targetLogic))
        {
            Debug.Log("뷰: 값이 들어 있다");
            return;
        }


        //0119 액션 아이템 손 바꿔서 들기 or 중복생성 방지를 위해 추가
        //    if (leftHandMagicData != null &&
        //targetLogic == _player.Inventory.GetActionInstance(leftHandMagicData))
        //    {
        //        Debug.Log("왼쪽이 빈 칸이 아니고 오른쪽에 왼쪽 데이터를 넣어서 막힘 즉 손 바꾸기");
        //        return;
        //    }
        //        if (rightHandMagicData != null &&
        //        targetLogic == _player.Inventory.GetActionInstance(rightHandMagicData))
        //    {
        //        Debug.Log("오른쪽이 빈 칸이 아니고 왼쪽에 오른쪽 데이터를 넣어서 막힘 즉 손 바꾸기");
        //        return;
        //    }

        //if (_player.Inventory.GetActionInstance(leftHandMagicData) == targetLogic ||
        //    _player.Inventory.GetActionInstance(rightHandMagicData) == targetLogic)
        //{
        //    return;
        //}

        //if (_actionCooltimeList.Contains(targetLogic))
        //{
        //    Debug.Log("값이 들어 있다");
        //    return;
        //}
        //_actionCooltimeList.Add(targetLogic);



        if (targetLogic != null && targetLogic.CurrentCooldown > 0)
        {
            SetMagicIcon(targetLogic);
        }
    }
    public void SetMagicIcon(ActionBase action)
    {
        GameObject magicIcon = GetMagicIcon();
        if (magicIcon == null) return;


        ActionItemDataSO leftHandMagicData = _boundSystem.LeftHandSlot as ActionItemDataSO;
        ActionItemDataSO rightHandMagicData = _boundSystem.RightHandSlot as ActionItemDataSO;
        ActionBase LeftLogic = _player.Inventory.GetActionInstance(leftHandMagicData);
        ActionBase rightLogic = _player.Inventory.GetActionInstance(rightHandMagicData);

        //이전 마법이 변경 후에도 왼손, 오른손에 들려있으면 반환
        //if (action == LeftLogic || action == rightLogic)
        //    return;
        //이미 키를 가지고 있으면 반환
        //if (_actionCooltimeDictionary.ContainsKey(action))
        //    return;

        _actionCooltimeDictionary[action] = magicIcon;
        Debug.Log("뷰: 아이콘 생성");


        magicIcon.transform.GetChild(0).GetComponent<Image>().sprite = action.BaseData.itemImage;
        Image coolTimeImage = magicIcon.transform.GetChild(0).GetChild(0).GetComponent<Image>();

        float curCoolTime = action.CurrentCooldown;
        float maxCoolTime = action.BaseData.cooldown;
        coolTimeImage.fillAmount = curCoolTime / maxCoolTime;
        magicIcon.SetActive(true);




        StartCoroutine(CheckCoolTimeOnInventory(coolTimeImage, curCoolTime, maxCoolTime, magicIcon, action));
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
                leftHandIcon.color = new Color(1, 1, 1, 0);
                leftHandIconCoolTime.fillAmount = 0;
            }
            else
            {
                rightHandIcon.sprite = null;
                rightHandIcon.color = new Color(1, 1, 1, 0);
                rightHandIconCoolTime.fillAmount = 0;
            }
            return;
        }

        if (data.itemImage == null)
            return;

        if (isLeft)
        {
            leftHandIcon.sprite = data.itemImage;
            leftHandIcon.color = new Color(1, 1, 1, 1);
            leftHandIconCoolTime.fillAmount = 0;
        }
        else
        {
            rightHandIcon.sprite = data.itemImage;
            rightHandIcon.color = new Color(1, 1, 1, 1);
            rightHandIconCoolTime.fillAmount = 0;
        }

        ActionItemDataSO actionData = data as ActionItemDataSO;
        if (actionData == null) return;
        ActionBase targetLogic = _player.Inventory.GetActionInstance(actionData);
        //if (_actionCooltimeList.Contains(targetLogic))
        //{
        //    _actionCooltimeList.Remove(targetLogic);
        //    //여기에 해당 아이콘 지우는 거 넣어야 하는데
        //    //리스트가 아니라 딕셔너리로 가져와야
        //}
        Debug.Log("뷰: 손에 아이콘 넣기");
        if (_actionCooltimeDictionary.ContainsKey(targetLogic))
        {
            Debug.Log("뷰: 지우기");
            _actionCooltimeDictionary[targetLogic].SetActive(false);
            _actionCooltimeDictionary.Remove(targetLogic);
            //이제 보니까 해당 아이콘 코루틴도 멈춰야 하네
            //StopCoroutine(_actionCooltimeCoroutineDictionary[targetLogic]);
            //_actionCooltimeCoroutineDictionary.Remove(targetLogic);
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

    public IEnumerator CheckCoolTimeOnInventory(Image checkCoolDown, float curCoolTime, float maxCoolTime, GameObject magicIcon, ActionBase action)
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

        if (!_actionCooltimeDictionary.ContainsKey(action))
            yield break;
        _actionCooltimeDictionary[action].SetActive(false);
        _actionCooltimeDictionary.Remove(action);
        //_actionCooltimeList.Remove(action);
        //if (magicIcon != null)
        //{
        //    magicIcon.SetActive(false);
        //}
    }
}