using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomManager : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// onJointRoom() : 방 참가 성공
    /// OnJoinRandomFailed() : 랜덤 방 참가 실패(대부분 방이 없음) => 방 생성 필요
    /// OnCreatedRoom() : 방 생성 성공(방장이 됨)
    /// </summary>
    [SerializeField] Button StartBtn;
    [SerializeField] Button ReadyBtn;
    [SerializeField] Toggle checkHiddenRoom;
    [SerializeField] Transform playerInfoTab;
    [SerializeField] GameObject playerInfo;

    Hashtable readyTable = new Hashtable();
    bool onReady=false;
    int readyCount =0 ;


    Dictionary<int, TextMeshProUGUI> playerStateDic = new Dictionary<int, TextMeshProUGUI>();
    Dictionary<int, GameObject> playerInfoDic = new Dictionary<int, GameObject>();
    private IEnumerator Start()
    {
        FirebaseAuthManager.Instance.RefreshUser();
        yield return new WaitUntil(() => PhotonNetwork.InRoom);//방에 입장했는지
        yield return null;
        InitReady();
        

        //yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);//방에 연결 및 준비되었는지
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
                if (p.CustomProperties.TryGetValue("Ready", out object isReady))
                {
                    if ((bool)isReady)
                    {
                        player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Ready";
                    }
                    else
                    {
                        player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Wait";
                    }
                }
                else
                {
                    Debug.Log("값을 가져오지 못함");
                }
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            StartBtn.gameObject.SetActive(true);
            checkHiddenRoom.gameObject.SetActive(true);
        }
        else
        {
            ReadyBtn.gameObject.SetActive(true);
            checkHiddenRoom.gameObject.SetActive(false);
        }

    }


    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient == true)
        {
            PhotonNetwork.LoadLevel("Game");//네트워크 상에서 씬 바꾸는 것
        }
    }

    public void InitReady()
    {
        readyTable["Ready"] = false;

        PhotonNetwork.LocalPlayer.SetCustomProperties(readyTable);

        Debug.Log("준비 상태 초기화");
    }
    public void ReadyGame()
    {
        onReady = !onReady;
        readyTable["Ready"] = onReady;

        PhotonNetwork.LocalPlayer.SetCustomProperties(readyTable);
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
            if (newPlayer.CustomProperties.TryGetValue("Ready", out object isReady))
            {
                if ((bool)isReady)
                {
                    player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Ready";
                }
                else
                {
                    player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Wait";
                }
            }
            else
            {
                Debug.Log("값을 가져오지 못함");
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
            if (p.CustomProperties.TryGetValue("Ready", out object isReady))
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
                else if ((bool)isReady)
                {
                    readyCount++;
                    playerStateDic[p.ActorNumber].text = "Ready";
                }
                else
                {
                    playerStateDic[p.ActorNumber].text = "Wait";
                }
            }
            else
            {
                Debug.Log("값을 가져오지 못함");
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
        }
        else
        {
            StartBtn.gameObject.SetActive(false);
            ReadyBtn.gameObject.SetActive(true);
            checkHiddenRoom.gameObject.SetActive(false);
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
        PhotonNetwork.CurrentRoom.IsVisible = checkHiddenRoom.isOn;
    }
}
