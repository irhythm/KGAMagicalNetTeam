using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;
    public GameObject LocalPlayer { get; set; }

    [SerializeField] Button StartBtn;
    [SerializeField] Button ReadyBtn;
    [SerializeField] Toggle checkHiddenRoom;
    [SerializeField] Toggle friendlyFire;
    [SerializeField] Transform playerInfoTab;
    [SerializeField] GameObject playerInfo;
    public FryingPanLogic fryingPanLogic;

    bool onReady = false;
    int readyCount = 0;


    Dictionary<int, TextMeshProUGUI> playerStateDic = new Dictionary<int, TextMeshProUGUI>();
    Dictionary<int, GameObject> playerInfoDic = new Dictionary<int, GameObject>();

    [SerializeField] GameObject roomTab;
    [SerializeField] private InputActionReference tabInput;

    public GameObject player;

    [SerializeField] AudioClip RoomAudio;

    [SerializeField] string uiName = "RoomMenu";
    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        SoundManager.Instance.PlayBGM(RoomAudio);
        //yield return new WaitUntil(() => FirebaseAuthManager.Instance != null);//파이어베이스 초기화 대기
        if (FirebaseAuthManager.Instance != null)
        {
            FirebaseAuthManager.Instance.RefreshUser();
        }
        yield return new WaitUntil(() => PhotonNetwork.InRoom);//방에 입장했는지
        yield return null;

        InitReady();

        Debug.Log("탭 테스트 1");


        Player[] players = PhotonNetwork.PlayerList;//방 속 사람을 받아옴
        foreach (var p in players)
        {
            Debug.Log("방 안의 사람들 목록: " + p.NickName);
            GameObject player = Instantiate(playerInfo, playerInfoTab);
            playerInfoDic.Add(p.ActorNumber, player);
            player.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = p.NickName;
            playerStateDic.Add(p.ActorNumber, player.transform.GetChild(1).GetComponent<TextMeshProUGUI>());
            if (p.IsMasterClient)
            {
                player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Host";
            }
            else
            {
                if (p.GetProps<bool>(NetworkProperties.PLAYER_READY))
                {
                    player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Ready";
                }
                else
                {
                    player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Wait";
                }
            }
        }

        Debug.Log("탭 테스트 2");
        checkHiddenRoom.isOn = !PhotonNetwork.CurrentRoom.IsVisible;
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.ONROOM, true);
            PhotonNetwork.CurrentRoom.IsOpen = true;
            StartBtn.gameObject.SetActive(true);
            checkHiddenRoom.gameObject.SetActive(true);
            friendlyFire.gameObject.SetActive(true);

            friendlyFire.isOn = PhotonNetwork.CurrentRoom.GetProps<bool>(NetworkProperties.FRIENDLYFIRE);

            InitGameRound();

            InitMoneyCount();
        }
        else
        {
            ReadyBtn.gameObject.SetActive(true);
            checkHiddenRoom.gameObject.SetActive(false);
        }
        Debug.Log("탭 테스트 3");
        CheckReady();

        if (tabInput != null)
        {
            Debug.Log("탭 인풋 있음");
            tabInput.action.performed += OpenRoomTab;
            tabInput.action.Enable();
        }
        else
        {
            Debug.Log("탭 인풋 없음");
        }

        Debug.Log("탭 테스트 4");
    }

    public void SetPlayer(GameObject player)
    {
        this.player = player;
    }

    private void OnDisable()
    {
        base.OnDisable();
        GameManager.Instance.isRoom=false;
        if (tabInput != null)
        {
            tabInput.action.performed -= OpenRoomTab;
        }
    }

    public void ChangeFriendlyFire()
    {
        PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.FRIENDLYFIRE, friendlyFire.isOn);
    }

    //나중에 값 받아올 것 대비 제작
    public void InitGameRound()
    {
        PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.GAMEROUND, 2);
    }

    //팀 재화 초기화 코드
    public void InitMoneyCount()
    {
        PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.MONEYCOUNT, 0);
    }
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient == true)
        {
            GameManager.Instance.ResetCustomProperty();
            PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.ONSTART, true);
        }
    }
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(NetworkProperties.ONSTART))
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            if (PhotonNetwork.IsMasterClient == true)
            {
                player = null;
                //PhotonNetwork.Destroy(player);
                PhotonNetwork.AutomaticallySyncScene = true;
                PhotonNetwork.LoadLevel("GameMapOne");//네트워크 상에서 씬 바꾸는 것
                PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.ONSTART, false);
                PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.ONROOM, false);
            }
        }
    }

    public void InitReady()
    {
        PhotonNetwork.LocalPlayer.SetProps(NetworkProperties.PLAYER_READY, false);
    }
    public void ReadyGame()
    {
        onReady = !onReady;
        PhotonNetwork.LocalPlayer.SetProps(NetworkProperties.PLAYER_READY, onReady);
        CheckReady();

        Debug.Log("준비 상태 변경");
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + "님이 방에 입장하셨습니다.");

        GameObject player = Instantiate(playerInfo, playerInfoTab);
        playerInfoDic.Add(newPlayer.ActorNumber, player);

        CheckReady();

        player.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = newPlayer.NickName;
        playerStateDic.Add(newPlayer.ActorNumber, player.transform.GetChild(1).GetComponent<TextMeshProUGUI>());
        if (newPlayer.IsMasterClient)
        {
            player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Host";
        }
        else
        {
            if (newPlayer.GetProps<bool>(NetworkProperties.PLAYER_READY))
            {
                player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Ready";
            }
            else 
            {
                player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Wait";
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        Debug.Log("준비 상태 변경");
        CheckReady();
    }

    public void CheckReady()
    {
        readyCount = 0;
        Player[] players = PhotonNetwork.PlayerList;
        foreach (var p in players)
        {
            if (!playerStateDic.ContainsKey(p.ActorNumber))
            {
                Debug.Log("해당 값이 없음");
                continue;
            }
            if (p.IsMasterClient)
            {
                readyCount++;
                playerStateDic[p.ActorNumber].text = "Host";
            }
            else if (p.GetProps<bool>(NetworkProperties.PLAYER_READY))
            {
                readyCount++;
                playerStateDic[p.ActorNumber].text = "Ready";
            }
            else
            {
                playerStateDic[p.ActorNumber].text = "Wait";
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount <= readyCount)
            {
                Debug.Log("시작 가능 현재 준비 플레이어 " + readyCount + "/" + PhotonNetwork.CurrentRoom.PlayerCount);
                StartBtn.interactable = true;
            }
            else
            {
                Debug.Log("시작 불가능 현재 준비 플레이어 " + readyCount + "/" + PhotonNetwork.CurrentRoom.PlayerCount);
                StartBtn.interactable = false;
            }
        }
    }
    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        playerStateDic.Remove(newPlayer.ActorNumber);
        Destroy(playerInfoDic[newPlayer.ActorNumber]);
        CheckReady();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        //포톤은 방장이 나가면 다른 사람이 방장이 된다
        Debug.Log(newMasterClient.NickName + "님이 방장이 되었습니다.");

        Room room = PhotonNetwork.CurrentRoom;

        if (PhotonNetwork.IsMasterClient)
        {
            StartBtn.gameObject.SetActive(true);
            ReadyBtn.gameObject.SetActive(false);
            checkHiddenRoom.gameObject.SetActive(true);
            friendlyFire.gameObject.SetActive(true);
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("FriendlyFire", out object isCheck))
                friendlyFire.isOn = (bool)isCheck;
            else
                PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.FRIENDLYFIRE, friendlyFire.isOn);
        }
        else
        {
            StartBtn.gameObject.SetActive(false);
            ReadyBtn.gameObject.SetActive(true);
            checkHiddenRoom.gameObject.SetActive(false);
            friendlyFire.gameObject.SetActive(false);
        }
    }

    //room property를 이용해 다른 인원의 레디 정보 등 불러올 수 있음


    public void SetPlayerReady(bool isReady)
    {
        PhotonNetwork.LocalPlayer.SetProps(NetworkProperties.PLAYER_READY, isReady);
    }

    public void ChangeRoomToHidden()
    {
        PhotonNetwork.CurrentRoom.IsVisible = !checkHiddenRoom.isOn;
    }

    public void CheckFiredlyFire()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.FRIENDLYFIRE, friendlyFire.isOn);
    }

    //260113 최정욱
    public void ReturnToLobby()
    {
        if (!PhotonNetwork.InRoom)
            return;
        if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Leaving)
            return;
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void OpenRoomTab(InputAction.CallbackContext context)
    {
        Debug.Log("탭 열기 시도");
        bool onOpen = !roomTab.activeSelf;
        if (onOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UIManager.Instance.OpenUI(uiName);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            UIManager.Instance.CloseUI(uiName);
        }

        roomTab.SetActive(onOpen);
    }
}


#region LegacyCode
/*
 * using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;
    public GameObject LocalPlayer { get; set; }

    [SerializeField] Button StartBtn;
    [SerializeField] Button ReadyBtn;
    [SerializeField] Toggle checkHiddenRoom;
    [SerializeField] Toggle friendlyFire;
    [SerializeField] Transform playerInfoTab;
    [SerializeField] GameObject playerInfo;
    public FryingPanLogic fryingPanLogic;

    Hashtable readyTable = new Hashtable();
    Hashtable roomTable = new Hashtable();
    bool onReady = false;
    int readyCount = 0;


    Dictionary<int, TextMeshProUGUI> playerStateDic = new Dictionary<int, TextMeshProUGUI>();
    Dictionary<int, GameObject> playerInfoDic = new Dictionary<int, GameObject>();

    [SerializeField] GameObject roomTab;
    [SerializeField] private InputActionReference tabInput;

    public GameObject player;

    [SerializeField] AudioClip RoomAudio;

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        SoundManager.Instance.PlayBGM(RoomAudio);
        //yield return new WaitUntil(() => FirebaseAuthManager.Instance != null);//파이어베이스 초기화 대기
        if (FirebaseAuthManager.Instance != null)
        {
            FirebaseAuthManager.Instance.RefreshUser();
        }
        yield return new WaitUntil(() => PhotonNetwork.InRoom);//방에 입장했는지
        yield return null;
        InitReady();

        Player[] players = PhotonNetwork.PlayerList;//방 속 사람을 받아옴
        foreach (var p in players)
        {
            Debug.Log("방 안의 사람들 목록: " + p.NickName);
            GameObject player = Instantiate(playerInfo, playerInfoTab);
            playerInfoDic.Add(p.ActorNumber, player);
            player.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = p.NickName;
            playerStateDic.Add(p.ActorNumber, player.transform.GetChild(1).GetComponent<TextMeshProUGUI>());
            if (p.IsMasterClient)
            {
                player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Host";
            }
            else
            {
                if (p.GetProps<bool>(NetworkProperties.PLAYER_READY))
                {
                    player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Ready";
                }
                else
                {
                    player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Wait";
                }
                //if (p.CustomProperties.TryGetValue("Ready", out object isReady))
                //{
                //    if ((bool)isReady)
                //    {
                //        player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Ready";
                //    }
                //    else
                //    {
                //        player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Wait";
                //    }
                //}
                //else
                //{
                //    Debug.Log("값을 가져오지 못함");
                //}
            }
        }

        checkHiddenRoom.isOn = !PhotonNetwork.CurrentRoom.IsVisible;
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            StartBtn.gameObject.SetActive(true);
            checkHiddenRoom.gameObject.SetActive(true);
            friendlyFire.gameObject.SetActive(true);

            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("FriendlyFire", out object isCheck))
            {
                friendlyFire.isOn = (bool)isCheck;
            }
            else
            {
                //roomTable["FriendlyFire"] = friendlyFire.isOn;
                //PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);

                PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.FRIENDLYFIRE, friendlyFire.isOn);
                // 로컬 플레이어의 프로퍼티를 업데이트 (네트워크 전체에 동기화됨)
            }

            //라운드는 기본으로 2 라운드 이후 해당 값이 깎이는 형식으로 진행
            //roomTable["GameRound"] = 2;
            //PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
            InitGameRound();

            InitMoneyCount();
        }
        else
        {
            ReadyBtn.gameObject.SetActive(true);
            checkHiddenRoom.gameObject.SetActive(false);
        }

        if (tabInput != null)
        {
            tabInput.action.performed += OpenRoomTab;
        }
    }

    public void SetPlayer(GameObject player)
    {
        this.player = player;
    }

    //해당 코드 실행시 게임 시작 버튼에 치명적인 버그 발생
    //private override void OnDisable()
    //{
    //    if (tabInput != null)
    //    {
    //        tabInput.action.performed -= OpenRoomTab;
    //    }
    //}
    private void OnDisable()
    {
        if (tabInput != null)
        {
            tabInput.action.performed -= OpenRoomTab;
        }
    }

    public void ChangeFriendlyFire()
    {
        //roomTable["FriendlyFire"] = friendlyFire.isOn;
        //PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
        PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.FRIENDLYFIRE, friendlyFire.isOn);
    }

    //나중에 값 받아올 것 대비 제작
    public void InitGameRound()
    {
        //roomTable["GameRound"] = 2;
        //PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
        PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.GAMEROUND, 2);
    }

    //팀 재화 초기화 코드
    public void InitMoneyCount()
    {
    //    roomTable["MoneyCount"] = 0;
    //    PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
        PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.MONEYCOUNT, 0);
    }
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient == true)
        {
            //roomTable["OnStore"] = true;
            GameManager.Instance.ResetCustomProperty();
            //roomTable["OnStart"] = true;
            //PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
            PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.ONSTART, true);
        }
    }
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("OnStart"))
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            if (PhotonNetwork.IsMasterClient == true)
            {
                PhotonNetwork.Destroy(player);
                PhotonNetwork.LoadLevel("GameMapOne");//네트워크 상에서 씬 바꾸는 것

            }
        }
    }

    public void InitReady()
    {
        PhotonNetwork.LocalPlayer.SetProps(NetworkProperties.PLAYER_READY, false);
        //readyTable["Ready"] = false;

        //PhotonNetwork.LocalPlayer.SetCustomProperties(readyTable);

        //Debug.Log("준비 상태 초기화");
    }
    public void ReadyGame()
    {
        onReady = !onReady;
        PhotonNetwork.LocalPlayer.SetProps(NetworkProperties.PLAYER_READY, onReady);
        //readyTable["Ready"] = onReady;

        //PhotonNetwork.LocalPlayer.SetCustomProperties(readyTable);
        CheckReady();

        Debug.Log("준비 상태 변경");
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + "님이 방에 입장하셨습니다.");

        GameObject player = Instantiate(playerInfo, playerInfoTab);
        playerInfoDic.Add(newPlayer.ActorNumber, player);

        CheckReady();

        player.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = newPlayer.NickName;
        playerStateDic.Add(newPlayer.ActorNumber, player.transform.GetChild(1).GetComponent<TextMeshProUGUI>());
        if (newPlayer.IsMasterClient)
        {
            player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Host";
        }
        else
        {
            if (newPlayer.GetProps<bool>(NetworkProperties.PLAYER_READY))
            {
                player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Ready";
            }
            else 
            {
                player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Wait";
            }
            //if (newPlayer.CustomProperties.TryGetValue("Ready", out object isReady))
            //{
            //    if ((bool)isReady)
            //    {
            //        player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Ready";
            //    }
            //    else
            //    {
            //        player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Wait";
            //    }
            //}
            //else
            //{
            //    Debug.Log("값을 가져오지 못함");
            //}
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        Debug.Log("준비 상태 변경");
        CheckReady();
    }

    public void CheckReady()
    {
        readyCount = 0;
        Player[] players = PhotonNetwork.PlayerList;
        foreach (var p in players)
        {
            if (!playerStateDic.ContainsKey(p.ActorNumber))
            {
                Debug.Log("해당 값이 없음");
                continue;
            }
            if (p.IsMasterClient)
            {
                readyCount++;
                playerStateDic[p.ActorNumber].text = "Host";
            }
            else if (p.GetProps<bool>(NetworkProperties.PLAYER_READY)
            {
                readyCount++;
                playerStateDic[p.ActorNumber].text = "Ready";
            }
            else
            {
                playerStateDic[p.ActorNumber].text = "Wait";
            }
        //if (p.CustomProperties.TryGetValue("Ready", out object isReady))
            //{
            //    if (!playerStateDic.ContainsKey(p.ActorNumber))
            //    {
            //        Debug.Log("해당 값이 없음");
            //        continue;
            //    }
            //    if (p.IsMasterClient)
            //    {
            //        readyCount++;
            //        playerStateDic[p.ActorNumber].text = "Host";
            //    }
            //    else if ((bool)isReady)
            //    {
            //        readyCount++;
            //        playerStateDic[p.ActorNumber].text = "Ready";
            //    }
            //    else
            //    {
            //        playerStateDic[p.ActorNumber].text = "Wait";
            //    }
            //}
            //else
            //{
            //    Debug.Log("값을 가져오지 못함");
            //}
        }
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount <= readyCount)
            {
                Debug.Log("시작 가능 현재 준비 플레이어 " + readyCount + "/" + PhotonNetwork.CurrentRoom.PlayerCount);
                StartBtn.interactable = true;
            }
            else
            {
                Debug.Log("시작 불가능 현재 준비 플레이어 " + readyCount + "/" + PhotonNetwork.CurrentRoom.PlayerCount);
                StartBtn.interactable = false;
            }
        }
    }
    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        playerStateDic.Remove(newPlayer.ActorNumber);
        Destroy(playerInfoDic[newPlayer.ActorNumber]);
        CheckReady();
        Debug.Log(newPlayer.NickName + "님이 방에서 퇴장하셨습니다.");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        //포톤은 방장이 나가면 다른 사람이 방장이 된다
        Debug.Log(newMasterClient.NickName + "님이 방장이 되었습니다.");

        Room room = PhotonNetwork.CurrentRoom;
        //룸에 진입하지 못하게 하는 코드
        //room.IsOpen = false;
        //룸이 로비씬에서 안보이도록 하는 코드
        //room.IsVisible = true;

        if (PhotonNetwork.IsMasterClient)
        {
            StartBtn.gameObject.SetActive(true);
            ReadyBtn.gameObject.SetActive(false);
            checkHiddenRoom.gameObject.SetActive(true);
            friendlyFire.gameObject.SetActive(true);
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("FriendlyFire", out object isCheck))
            {
                friendlyFire.isOn = (bool)isCheck;
            }
            else
            {
                //roomTable["FriendlyFire"] = friendlyFire.isOn;
                //PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
                PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.FRIENDLYFIRE, friendlyFire.isOn);
            }
        }
        else
        {
            StartBtn.gameObject.SetActive(false);
            ReadyBtn.gameObject.SetActive(true);
            checkHiddenRoom.gameObject.SetActive(false);
            friendlyFire.gameObject.SetActive(false);
        }
    }

    //room property를 이용해 다른 인원의 레디 정보 등 불러올 수 있음


    public void SetPlayerReady(bool isReady)
    {
        Hashtable props = new Hashtable
    {
        { "IsReady", isReady }
    };

        // 로컬 플레이어의 프로퍼티를 업데이트 (네트워크 전체에 동기화됨)
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }


    public void ChangeRoomToHidden()
    {
        PhotonNetwork.CurrentRoom.IsVisible = !checkHiddenRoom.isOn;
    }

    public void CheckFiredlyFire()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;


        //roomTable["FriendlyFire"] = friendlyFire.isOn;
        //PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
        PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.FRIENDLYFIRE, friendlyFire.isOn);
        // 로컬 플레이어의 프로퍼티를 업데이트 (네트워크 전체에 동기화됨)
    }

    //260113 최정욱
    public void ReturnToLobby()
    {
        if (!PhotonNetwork.InRoom)
            return;
        if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Leaving)
            return;
        PhotonNetwork.LeaveRoom();

    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void OpenRoomTab(InputAction.CallbackContext context)
    {
        bool onOpen = !roomTab.activeSelf;
        if (onOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        roomTab.SetActive(onOpen);
    }

}

 */
#endregion