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
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Unity.VisualScripting;
public class GameManager : PhotonSingleton<GameManager>
{
    public GameObject LocalPlayer { get; set; }

    public static GameManager Instance;
    [SerializeField] GameObject playerPrefab;

    [SerializeField] InventoryWheelLogic _inventoryWheelLogic;
    public InventoryWheelLogic InventoryWheel => _inventoryWheelLogic;

    Hashtable roomTable = new Hashtable();

    //PhotonView pv;

    [SerializeField] int needMoneyCount = 5;

    void Start()//씬이 너무 빨리 불러와져서 스타트가 room 들어가기 전에 호출되는 것이 문제임
    {
        //pv=GetComponent<PhotonView>();
        Instance = this;//실체도 없고 그냥 스크립트로만 존재해서 간단히 제작
        if (PlayerManager.LocalPlayerInstance == null)//플레이어 매니저가 이미 플레이어 정보를 들고있을 경우 패스
        {
            StartCoroutine(SpawnPlayerWhenConnected());
        }

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    IEnumerator SpawnPlayerWhenConnected() //네트워크 게임은, 라이프 사이클도 중요하고, 또 네트워크 지연까지 고려해야 함
    {
        yield return new WaitUntil(() => PhotonNetwork.InRoom);

        GameObject player = PhotonNetwork.Instantiate("PlayerPrefab/" + playerPrefab.name, new Vector3(0f, 1f, 0f), Quaternion.identity, 0);
        PlayerManager.LocalPlayerInstance = player;

        CheckInGamePlayer();

        yield return new WaitUntil(() => UIManager.Instance != null);
        InitMoneyCountAndStore();
    }

    //다음 씬을 넘어갈수 있는지 확인하는 코드
    public bool CheckMoneyCount()
    {
        //현재 씬이 상점 씬이면 재화 체크 필요 없음
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("OnStore", out object onStore))
        {
            bool checkStore = (bool)onStore;
            if (checkStore)
                return true;
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("MoneyCount", out object count))
        {
            int curMoneyCount = (int)count;
            if (curMoneyCount >= needMoneyCount)
            {
                return true;
            }
        }
        return false;
    }

    public void UseTeamMoney(int moneyCount)
    {
        int result = CurTeamMoney() - moneyCount;
        roomTable["MoneyCount"] = result;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
    }

    public int CurTeamMoney()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("MoneyCount", out object count))
        {
            return (int)count;
        }
        else
        {
            Debug.Log("불러오기 실패");
            return -1;
        }
    }

    //게임 라운드 넘어갈 경우 해당 팀 재화를 일정 수 만큼 깎는 코드 (상점씬에서는 재화를 깎지 않아야 함) <- 해당 코드는 플레이어 소환 전에 실행
    public void InitMoneyCountAndStore()
    {
        int result = -1;

        //해당 씬은 씬을 넘어가고 나서 발동함 <- 그러므로 게임 -> 상점 (발동) 상점 -> 게임 (발동 X) 즉 현재 상점씬이어야 발동 근데 룸 프로퍼티가 업데이트가 꼬여서인지 현재 상점씬 아닐 경우에 발동함
        //보니까 씬을 넘어가고 나서 커스텀 프로퍼티를 변경함 이로 인해 꼬임 현상 발동
        //그냥 여기서 스토어도 같이 초기화 시키자
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("MoneyCount", out object count))
        {
            Debug.Log("팀 재화 받아옴");
            result = (int)count;

        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("OnStore", out object onStore))
        {
            bool checkStore = (bool)onStore;
            Debug.Log("현재 상점 : "+checkStore);
            if (!checkStore)
            {
                //처음 시작했을 때 대비용
                if (result >= needMoneyCount)
                {
                    result = result - needMoneyCount;
                    if (PhotonNetwork.IsMasterClient)
                    {
                        roomTable["MoneyCount"] = result;
                        PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
                    }
                }
            }
            roomTable["OnStore"] = !checkStore;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
        }
        else 
        {
            Debug.Log("상점 여부 지정");
            roomTable["OnStore"] = false;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
        }


        if (UIManager.Instance.moneyCount != null)
        {
            UIManager.Instance.moneyCount.text = result + "";
            Debug.Log("팀 재화 표시");
        }
        else
        {
            Debug.Log("팀 재화 표시 실패");

        }
    }

    public void PlusMoneyCount()
    {

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("MoneyCount", out object count))
        {
            int curMoneyCount = (int)count + 1;

            if (PhotonNetwork.IsMasterClient)
            {
                roomTable["MoneyCount"] = curMoneyCount;
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
            }
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("MoneyCount"))
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("MoneyCount", out object count))
            {
                if (UIManager.Instance.moneyCount != null)
                {
                    UIManager.Instance.moneyCount.text = (int)count + "";
                }
            }
        }
    }

    public void CheckInGamePlayer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //해당 코드는 플레이어가 소환될 때마다 실행되어야 함(매번 살아나니까 룸 내의 플레이어 수 만큼 초기화)
            //참고로 해당 코드는 플레이어가 disable(나갈 경우) 때도 실행되어야 함
            roomTable["PlayerCount"] = PhotonNetwork.CurrentRoom.PlayerCount;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
        }
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
        CheckInGamePlayer();
    }

    public void ExitGame()
    {
        LeaveRoom();
        SceneManager.LoadSceneAsync("Lobby");
    }

    public void CheckDie()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("PlayerCount", out object count))
        {
            roomTable["PlayerCount"] = (int)count - 1;
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
    }

    public void CheckRoundClear(int player)
    {
        if (!CheckMoneyCount())
        {
            Debug.Log("재화량을 충족시키지 못함");
            return;
        }
       
        int maxPlayer = 0;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("PlayerCount", out object count))
        {
            maxPlayer = (int)count;
        }
        else
        {
            Debug.Log("클리어 인원 수를 가져오지 못함");
        }
        Debug.Log($"인원 {player}/{maxPlayer} ");
        if (player < maxPlayer)
            return;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameRound", out object round))
        {
            int curRound = (int)round;
            curRound--;

            //PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
            if (curRound > 0)
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("OnStore", out object onStore))
                {
                    bool checkStore = (bool)onStore;
                    if (checkStore)
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            //게임씬으로 이동
                            PhotonNetwork.LoadLevel("GameMapOne");
                            //StartCoroutine(InitMoneyCountAndStore());
                            roomTable["GameRound"] = curRound;
                        }
                    }
                    else
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            PhotonNetwork.LoadLevel("StoreMapSensei");
                            //StartCoroutine(InitMoneyCountAndStore());
                        }
                    }
                }
                else
                {
                    Debug.Log("상점 여부를 가져오지 못함");
                }
            }
            else
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    //모든 라운드를 소비하였으므로 Win
                    PhotonNetwork.LoadLevel("Win");
                    ResetCustomProperty();
                }
            }
        }
        else
        {
            Debug.Log("라운드 정보를 가져오지 못함");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex >= 4 && scene.buildIndex <= SceneManager.sceneCountInBuildSettings-2)
        {
            StartCoroutine(SpawnPlayerWhenConnected());
        }
    }

    public void ResetCustomProperty()
    {
        //마스터가 커스텀 프로퍼티 목록들 초기화
        if (!PhotonNetwork.IsMasterClient)
            return;
        //어차피 플레이어 커스텀 프로퍼티는 씬 이동시 roommanager에서 초기화된다 그러므로 room만 초기화
        //foreach (var key in roomTable.Keys)
        //{
        //    roomTable.Remove(key);
        //} // <- 해당 방식으로 초기화가 정상적으로 이루어지지 않음
        roomTable["MoneyCount"] = 0;
        roomTable["OnStore"] = true;
        roomTable["GameRound"] = 2;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }
    }

}
