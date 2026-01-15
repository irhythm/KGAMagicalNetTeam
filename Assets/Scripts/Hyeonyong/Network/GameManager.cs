using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    [SerializeField] GameObject playerPrefab;
    //[SerializeField] GameObject playerInfoPrefab;
    //[SerializeField] Transform playerInfoPanel;

    

    //PlayerInput playerInput;
    [Header("인풋 액션")]
    [SerializeField] private InputActionReference playerInput;
    //260115 최정욱 인벤토리 UI 관련 추가
    [SerializeField] List<InputActionReference> _inventoryInputActions; //0은 q, 1은 e
    [SerializeField] GameObject _inventoryUI;
    bool _isInventoryUIOn = false;
    int _qOrE = 0; //0은 q, 1은 e

    void OpenInventory(InputAction.CallbackContext context)
    {
        if (_inventoryUI == null)
            return;
        if (!_isInventoryUIOn)
        {
            if (context.action == _inventoryInputActions[0].action)
            {
                _qOrE = 0;
            }
            else if (context.action == _inventoryInputActions[1].action)
            {
                _qOrE = 1;
            }
            _isInventoryUIOn = !_isInventoryUIOn;
            _inventoryUI.SetActive(_isInventoryUIOn);
            PlayerManager.LocalPlayerInstance.GetComponent<PlayerCameraSetup>().cameraScript.SetControl(false);
        }
    }

    void CloseInventory(InputAction.CallbackContext context)
    {
        if (_inventoryInputActions[_qOrE].action != context.action)
        {
            return;
        }
        _isInventoryUIOn = !_isInventoryUIOn;
        _inventoryUI.SetActive(_isInventoryUIOn);
        PlayerManager.LocalPlayerInstance.GetComponent<PlayerCameraSetup>().cameraScript.SetControl(true);
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
    


    [SerializeField] GameObject gameSettingUI;
    bool onGameSettingUI=false;

    public Action onOpenUI;
    public Action onCloseUI;

    Dictionary<string, bool> checkUI = new Dictionary<string, bool>();


    [SerializeField] string uiName = "GameMenu";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()//씬이 너무 빨리 불러와져서 스타트가 room 들어가기 전에 호출되는 것이 문제임
    {
        Instance = this;//실체도 없고 그냥 스크립트로만 존재해서 간단히 제작
        if (PlayerManager.LocalPlayerInstance==null)//플레이어 매니저가 이미 플레이어 정보를 들고있을 경우 패스
        {
            StartCoroutine(SpawnPlayerWhenConnected());
        }
        //playerInput = GetComponent<PlayerInput>();
        //playerInput.actions["Exit"].performed += OpenUI;

        AddUI(uiName);

        playerInput.action.Enable();
        playerInput.action.performed += OpenUI;

        //260115 최정욱 인벤토리 UI 관련 추가
        //_inventoryInputActions
        _inventoryInputActions[0].action.Enable();
        _inventoryInputActions[0].action.performed += OpenInventory;
        _inventoryInputActions[1].action.Enable();
        _inventoryInputActions[1].action.performed += OpenInventory;

        _inventoryInputActions[0].action.canceled += CloseInventory;
        _inventoryInputActions[1].action.canceled += CloseInventory;

    }


    IEnumerator SpawnPlayerWhenConnected() //네트워크 게임은, 라이프 사이클도 중요하고, 또 네트워크 지연까지 고려해야 함
    {
        yield return new WaitUntil(() => PhotonNetwork.InRoom);
        //PlayerManager.LocalPlayerInstance = PhotonNetwork.Instantiate("PlayerPrefab/" + playerPrefab.name, new Vector3(0f, 1f, 0f), Quaternion.identity, 0);

        GameObject player = PhotonNetwork.Instantiate("PlayerPrefab/" + playerPrefab.name, new Vector3(0f, 1f, 0f), Quaternion.identity, 0);
        PlayerManager.LocalPlayerInstance = player;



        //UI라서 PhotonNetwork.Instantiate 안쓴다고 함
        /*
        GameObject playerInfo = PhotonNetwork.Instantiate(playerInfoPrefab.name, Vector3.zero,Quaternion.identity);
        playerInfo.transform.SetParent(playerInfoPanel);



        PlayerController playerController = player.GetComponent<PlayerController>();

        playerController.SetPlayerInfo(
            playerInfo.transform.GetChild(0).GetComponent<TextMeshProUGUI>(),
            playerInfo.transform.GetChild(1).GetComponent<Image>()
            );
        playerController.SetPlayerName(PhotonNetwork.NickName);*/
        //Player[] players = PhotonNetwork.PlayerList;//방 속 사람을 받아옴
        //foreach (var p in players)
        //{
        //    GameObject playerInfo = Instantiate(playerInfoPrefab, playerInfoPanel);

        //    PlayerController playerController = p.LocalPlayerInstance.GetComponent<PlayerController>();

        //    playerController.SetPlayerInfo(
        //        playerInfo.transform.GetChild(0).GetComponent<TextMeshProUGUI>(),
        //        playerInfo.transform.GetChild(1).GetComponent<Image>()
        //        );
        //    playerController.SetPlayerName(PhotonNetwork.NickName);
        //}
    }

    private void OnDisable()
    {
        //playerInput.actions["Exit"].performed -= OpenUI;
        playerInput.action.Disable();
    }
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(1);
    }

    public void LeaveRoom()
    {
        if (!PhotonNetwork.InRoom)
            return;
        if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Leaving)
            return;
        PhotonNetwork.LeaveRoom();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + "님이 게임을 떠났습니다.");
        if (PhotonNetwork.CountOfPlayersInRooms == 1)
        {
            Debug.Log("[승리] 현재 플레이어 수 : " + PhotonNetwork.CountOfPlayersInRooms);
        }
    }

    IEnumerator Winner()
    {
        yield return new WaitForSeconds(3f);
        PhotonNetwork.LoadLevel("RoomScene");//네트워크 상에서 씬 바꾸는 것
    }

    public void ExitGame()
    {
        LeaveRoom();
        SceneManager.LoadSceneAsync("Lobby");
    }

    private void OpenUI(InputAction.CallbackContext context)
    {
        Debug.Log("esc 입력");
        onGameSettingUI = !onGameSettingUI;
        gameSettingUI.SetActive(onGameSettingUI);

        if (onGameSettingUI)
        {
            OpenUI(uiName);
        }
        else
        {
            CloseUI(uiName);
        }
    }

    public void OpenUI(string uiName)
    {
        Debug.Log("UI 열림");
        checkUI[uiName] = true;
        onOpenUI.Invoke();
    }

    public void CloseUI(string name)
    {
        checkUI[name] = false;
        if (!CheckUiClose())
            return;
        Debug.Log("UI 닫힘");
        onCloseUI.Invoke();
    }

    public bool CheckUiClose()
    {
        foreach (var c in checkUI)
        {
            if (c.Value)
            {
                return false;
            }
        }
        return true;
    }
    public void AddUI(string name)
    {
        if (checkUI.ContainsKey(name))
            return;
        checkUI[name] = false;
    }

}
