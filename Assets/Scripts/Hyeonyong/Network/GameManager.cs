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
public class GameManager : PhotonSingleton<GameManager>
{
    public static GameManager Instance;
    [SerializeField] GameObject playerPrefab;

    [SerializeField] InventoryWheelLogic _inventoryWheelLogic;
    public InventoryWheelLogic InventoryWheel => _inventoryWheelLogic;

    Hashtable roomTable = new Hashtable();

    void Start()//씬이 너무 빨리 불러와져서 스타트가 room 들어가기 전에 호출되는 것이 문제임
    {
        Instance = this;//실체도 없고 그냥 스크립트로만 존재해서 간단히 제작
        if (PlayerManager.LocalPlayerInstance==null)//플레이어 매니저가 이미 플레이어 정보를 들고있을 경우 패스
        {
            StartCoroutine(SpawnPlayerWhenConnected());
        }
    }


    IEnumerator SpawnPlayerWhenConnected() //네트워크 게임은, 라이프 사이클도 중요하고, 또 네트워크 지연까지 고려해야 함
    {
        yield return new WaitUntil(() => PhotonNetwork.InRoom);

        Debug.Log("플레이어 소환");
        GameObject player = PhotonNetwork.Instantiate("PlayerPrefab/" + playerPrefab.name, new Vector3(0f, 1f, 0f), Quaternion.identity, 0);
        PlayerManager.LocalPlayerInstance = player;

        CheckInGamePlayer();
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
        if (PhotonNetwork.CountOfPlayersInRooms == 1)
        {
            Debug.Log("[승리] 현재 플레이어 수 : " + PhotonNetwork.CountOfPlayersInRooms);
        }
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
        if (player >= maxPlayer)
        {
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
                            //게임씬으로 이동
                            PhotonNetwork.LoadLevel("GameMapOne");
                            roomTable["OnStore"] = false;
                            roomTable["GameRound"] = curRound;
                            Debug.Log("플레이어 소환 시도");
                            StartCoroutine(SpawnPlayer());
                        }
                        else
                        {
                            PhotonNetwork.LoadLevel("StoreMapSensei");
                            roomTable["OnStore"] = true;
                            Debug.Log("플레이어 소환 시도");
                            StartCoroutine(SpawnPlayer());
                        }
                    }
                    else
                    {
                        Debug.Log("상점 여부를 가져오지 못함");
                    }
                }
                else
                {
                    //모든 라운드를 소비하였으므로 Win
                    PhotonNetwork.LoadLevel("Win");
                    ResetCustomProperty();
                }
            }
            else
            {
                Debug.Log("라운드 정보를 가져오지 못함");
            }
        }
    }

    private IEnumerator SpawnPlayer()
    {
        Debug.Log("플레이어 소환 시도 코루틴 진입");
        while (PhotonNetwork.LevelLoadingProgress < 1f)
        {
            Debug.Log("플레이어 소환 시도 코루틴 ");
            yield return null;
        }

        yield return null;
        Debug.Log("플레이어 소환 직전");
        StartCoroutine(SpawnPlayerWhenConnected());
    }

    public void ResetCustomProperty()
    {
        //마스터가 커스텀 프로퍼티 목록들 초기화
        if (!PhotonNetwork.IsMasterClient)
            return;
        //어차피 플레이어 커스텀 프로퍼티는 씬 이동시 roommanager에서 초기화된다 그러므로 room만 초기화
        foreach (var key in roomTable.Keys)
        {
            roomTable[key] = null;
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
    }

}
