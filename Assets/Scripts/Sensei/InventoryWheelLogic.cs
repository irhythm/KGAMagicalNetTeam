using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;
public class InventoryWheelLogic : MonoBehaviour
{
    //[SerializeField] List<Transform> _wheelSlotPositions; // 인벤토리 휠 슬롯들
    [SerializeField] List<Image> _wheelSlots; // 인벤토리 휠 슬롯들에 들어갈 아이템들
    [SerializeField] List<TextMeshProUGUI> _wheelSlotCounts; // 인벤토리 휠 슬롯들에 들어갈 아이템 갯수들

    //260115 최정욱 인벤토리 UI 관련 추가
    [Header("인벤토리 UI Sensei")]
    [SerializeField] List<InputActionReference> _inventoryInputActions; //0은 q, 1은 e
    [SerializeField] GameObject _inventoryUI;
    [SerializeField] InventoryWheelLogic _inventoryWheelLogic;
    public InventoryWheelLogic InventoryWheel => _inventoryWheelLogic;
    bool _isInventoryUIOn = false;
    public bool IsInventoryUIOn => _isInventoryUIOn;

    int _qOrE = 0; //0은 q, 1은 e



    //public GameObject ourPlayerReference;

    void OpenInventory(bool left)
    {
        Debug.Log("인벤토리 열림");
        if (_inventoryUI == null)
            return;
        if (!_isInventoryUIOn)
        {
            if (left)
            {
                _qOrE = 0;
            }
            else if (!left)
            {
                _qOrE = 1;
            }
            _isInventoryUIOn = !_isInventoryUIOn;
            _inventoryUI.SetActive(_isInventoryUIOn);
            GameManager.Instance.LocalPlayer.GetComponent<PlayerCameraSetup>().cameraScript.SetControl(false);
        }
    }



    IEnumerator Start()
    {
        yield return new WaitUntil(() => GameManager.Instance != null && GameManager.Instance.LocalPlayer != null);
        yield return new WaitUntil(() => GameManager.Instance.LocalPlayer.GetComponent<PlayerInputHandler>() != null);
        GameManager.Instance.InventoryWheel = this;
        //yield return new WaitUntil(() => ourPlayerReference != null && ourPlayerReference.GetComponent<PlayerInputHandler>() != null);

        GameManager.Instance.LocalPlayer.GetComponent<PlayerInputHandler>().OnSelectQorEEvent += OpenInventory;

        //Debug.Log("인벤토리 휠 로직 스타트에서 플레이어 레퍼런스 할당됨");
        if (GameManager.Instance.TemporaryPlayerInventory != null)
        {
            foreach (var keyvaluepair in GameManager.Instance.TemporaryPlayerInventory)
            {


                Debug.Log($"인벤토리 보존 {keyvaluepair.Key.itemName} : {keyvaluepair.Value}");

                for (int i = 0; i < keyvaluepair.Value; i++)
                {
                    GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().Inventory.AddItem(keyvaluepair.Key);
                }

                //PlayerInventory.Add(keyvaluepair.Key, keyvaluepair.Value);

                //GameManager.Instance.LocalPlayer.GetComponent<PlayerInventory>().Inventory

                //playerTable[keyvaluepair.Key.itemName] = keyvaluepair.Value;
            }
            GameManager.Instance.TemporaryPlayerInventory.Clear();

        }

        foreach (var item in GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().Inventory.Inventory)
        {
            Debug.Log("인벤토리 아이템들: " + item.Key.itemName + " , " + item.Value);
        }
        GameManager.Instance.LocalPlayer.GetComponent<PlayerInputHandler>().OnDeselectQorEEvent += CloseInventory;
        //260115 최정욱 인벤토리 UI 관련 추가
        //_inventoryInputActions
        //_inventoryInputActions[0].action.Enable();
        //_inventoryInputActions[0].action.performed += OpenInventory;
        //_inventoryInputActions[1].action.Enable();
        //_inventoryInputActions[1].action.performed += OpenInventory;
        UpdateWheelInventory();
        //_inventoryInputActions[0].action.canceled += CloseInventory;
        //_inventoryInputActions[1].action.canceled += CloseInventory;
    }

   

    void OnDestroy()
    {
        if (GameManager.Instance == null || GameManager.Instance.LocalPlayer == null)
            return;
        GameManager.Instance.LocalPlayer.GetComponent<PlayerInputHandler>().OnSelectQorEEvent -= OpenInventory;
        GameManager.Instance.LocalPlayer.GetComponent<PlayerInputHandler>().OnDeselectQorEEvent -= CloseInventory;

        //260115 최정욱 인벤토리 UI 관련 추가
        //_inventoryInputActions
        //_inventoryInputActions[0].action.performed -= OpenInventory;
        //_inventoryInputActions[1].action.performed -= OpenInventory;
        //_inventoryInputActions[0].action.canceled -= CloseInventory;
        //_inventoryInputActions[1].action.canceled -= CloseInventory;
    }
    void CloseInventory(bool left)
    {
        int temp;

        if (left)
        {
            temp = 0;
        }
        else
        {
            temp = 1;
        }

        if (_qOrE != temp)
        {
            return;
        }
        if (_inventoryWheelLogic != null)
        {
            switch (_qOrE)
            {
                case 0:
                    if (_inventoryWheelLogic.HoveredItem == null)
                    {
                        break;
                    }
                    if (GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.LeftHandSlot == null)
                    {
                        if (GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.RightHandSlot == null)
                        {

                            GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, true);
                            break;
                        }



                        else if (HoveredItem.itemName != GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.RightHandSlot.itemName)

                        {
                            GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, true);
                            break;
                        }
                        else if (HoveredItem.itemName == GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.RightHandSlot.itemName)
                        {
                            GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, true);
                            GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(null, false);
                            break;
                        }
                    }
                    else if (HoveredItem.itemName != GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.LeftHandSlot.itemName)
                    {
                        if (GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.RightHandSlot == null)
                        {
                            GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, true);
                            break;
                        }
                        else if (HoveredItem.itemName != GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.RightHandSlot.itemName)
                        {
                            GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, true);
                            break;
                        }
                        else if (HoveredItem.itemName == GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.RightHandSlot.itemName)
                        {
                            GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, true);
                            GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(null, false);
                            
                        }
                        //PlayerManager.LocalPlayerInstance.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, true);
                    }
                    else if (HoveredItem.itemName == GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.LeftHandSlot.itemName)
                    {
                        break;
                        //PlayerManager.LocalPlayerInstance.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, true);
                    }
                    


                    //_inventoryWheelLogic.SelectCurrentSlotByQ();
                    break;
                case 1:
                    Debug.Log("E 닫힘");
                    if (_inventoryWheelLogic.HoveredItem == null)
                    {
                        break;
                    }
                    if (GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.RightHandSlot == null)
                    {
                        Debug.Log("E 장착 One");
                        if (GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.LeftHandSlot == null)
                        {
                            GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, false);
                            break;
                        }
                        else if (HoveredItem.itemName != GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.LeftHandSlot.itemName)
                        {
                            GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, false);
                            Debug.Log("E 장착 Two");
                            break;
                        }
                        else if (HoveredItem.itemName == GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.LeftHandSlot.itemName)
                        {
                            GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, false);
                            GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(null, true);
                            break;
                        }
                        //PlayerManager.LocalPlayerInstance.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, false);
                        //break;
                    }
                    else if (HoveredItem.itemName != GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.RightHandSlot.itemName)
                    {
                        if (GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.LeftHandSlot == null)
                        {
                            GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, false);
                            break;
                        }
                        else if (HoveredItem.itemName != GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.LeftHandSlot.itemName)
                        {
                            GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, false);
                            break;
                        }
                        else if (HoveredItem.itemName == GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.LeftHandSlot.itemName)
                        {
                            GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, false);
                            GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(null, true);
                            break;
                        }

                        // PlayerManager.LocalPlayerInstance.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, false);
                    }
                    
                    else if (HoveredItem.itemName == GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().MagicSystem.RightHandSlot.itemName)
                    {
                        break;
                        //break;
                    }
                    //PlayerManager.LocalPlayerInstance.GetComponent<PlayableCharacter>().MagicSystem.EquipItem(_inventoryWheelLogic.HoveredItem, false);
                    //_inventoryWheelLogic.SelectCurrentSlotByE();
                    break;
            }
        }
        _isInventoryUIOn = !_isInventoryUIOn;
        _inventoryUI.SetActive(_isInventoryUIOn);
        GameManager.Instance.LocalPlayer.GetComponent<PlayerCameraSetup>().cameraScript.SetControl(true);
        //if (EventSystem.current.IsPointerOverGameObject())
        //{
        //    PointerEventData mouseEventData = new PointerEventData(EventSystem.current);
        //    if (Mouse.current != null)
        //    {
        //        mouseEventData.position = Mouse.current.position.ReadValue();
        //    }

        //    List<RaycastResult> pointerRaycastHits = new List<RaycastResult>();
        //    EventSystem.current.RaycastAll(mouseEventData, pointerRaycastHits);

        //    //if (pointerRaycastHits.Count > 0)
        //    //{
        //    //    for (int i = 0; i < pointerRaycastHits.Count; i++)
        //    //    {
        //    //        //Debug.Log("Hit UI: " + pointerRaycastHits[i].gameObject.name);
        //    //        for (int j = 0; j < 10; j++)
        //    //        {
        //    //            if (pointerRaycastHits[i].gameObject == _quickSlotBox[j])
        //    //            {
        //    //                //_hoveredUIIndex = j;
        //    //                return pointerRaycastHits[i].gameObject;
        //    //            }
        //    //        }

        //    //    }

        //    //    //return pointerRaycastHits[0].gameObject;
        //    //}
        //}
    }


    //더 정교한 인벤토리 휠을 위해 나중에 수정 필요
    public void UpdateWheelInventory()
    {
        //_wheelSlots.Clear();
        for (int i = 0; i<_wheelSlots.Count; i++)
        {
            if (_wheelSlots[i].sprite != null)
            {
                _wheelSlots[i].gameObject.SetActive(false);
                _wheelSlotCounts[i].text = "";

                _wheelSlots[i].sprite = null;
            }
        }


        int index = 0;
        foreach (var item in GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().Inventory.Inventory)
        {
            _wheelSlots[index].gameObject.SetActive(true);
            _wheelSlots[index].sprite = item.Key.itemImage;
            _wheelSlotCounts[index].text = item.Value.ToString();
            index++;

        }
    }



   

    //public Inventor
    public InventoryDataSO HoveredItem;

    // Update is called once per frame
    void Update()
    {
        if (!IsInventoryUIOn)
        {
            HoveredItem = null;
            return;
        }
        if (EventSystem.current.IsPointerOverGameObject())
        {
            PointerEventData mouseEventData = new PointerEventData(EventSystem.current);
            if (Mouse.current != null)
            {
                mouseEventData.position = Mouse.current.position.ReadValue();
            }

            List<RaycastResult> pointerRaycastHits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(mouseEventData, pointerRaycastHits);
            //foreach (var hit in pointerRaycastHits)
            //{
            //    Debug.Log("Hit UI CHECCCKK: " + hit.gameObject.name);
            //}

            if (pointerRaycastHits.Count > 0)
            {
                int index = 0;
                //Debug.Log("Hit UI: " + pointerRaycastHits[0].gameObject.name);
                //if (pointerRaycastHits.Count > 1)
                //{
                //    Debug.Log("Hit UI: " + pointerRaycastHits[1].gameObject.name);
                //}
                //if (pointerRaycastHits.Count > 2)
                //{
                //    Debug.Log("Hit UI: " + pointerRaycastHits[2].gameObject.name);
                //}
                //Debug.Log("UI Hit Object "+ pointerRaycastHits[index].gameObject.name);
                if (pointerRaycastHits[index].gameObject.GetComponent<Image>() == null)
                {
                    index++;
                    //Debug.Log("UI Hit Object " + pointerRaycastHits[index].gameObject.name);
                }

                if (pointerRaycastHits[index].gameObject.GetComponent<Image>() != null && pointerRaycastHits[index].gameObject.GetComponent<Image>().sprite != null)
                {
                    foreach (var item in GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().Inventory.Inventory)
                    {
                        if (item.Key.itemImage == pointerRaycastHits[index].gameObject.GetComponent<Image>().sprite)
                        {
                            HoveredItem = item.Key;
                            return;
                        }
                    }
                }
                //for (int i = 0; i < pointerRaycastHits.Count; i++)
                //{
                //    //Debug.Log("Hit UI: " + pointerRaycastHits[i].gameObject.name);
                //    for (int j = 0; j < 10; j++)
                //    {
                //        if (pointerRaycastHits[i].gameObject == _quickSlotBox[j])
                //        {
                //            //_hoveredUIIndex = j;
                //            return pointerRaycastHits[i].gameObject;
                //        }
                //    }

                //}

                //return pointerRaycastHits[0].gameObject;
            }
        }
        else
        {
            HoveredItem = null;
        }
    }
}
