using Firebase.Auth;//실시간으로 해야 할 것들
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] string roomSceneName;
    [SerializeField] TMP_InputField createRoomInput;
    [SerializeField] TMP_InputField joinRoomInput;
    [SerializeField] TMP_InputField changeNicknameInput;

    [SerializeField] GameObject roomPrefab;
    [SerializeField] Transform roomListPanel;
    //[SerializeField] List<string> curRoomList = new List<string>();
    [SerializeField] Dictionary<string,GameObject> curRoomList = new Dictionary<string, GameObject>();

    [SerializeField] AudioClip LobbyAudio;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        SoundManager.Instance.PlayBGM(LobbyAudio);
        Debug.Log("로비 씬 시작");
        if (FirebaseAuthManager.Instance != null)
        {
            FirebaseAuthManager.Instance.RefreshUser();
            if (FirebaseAuthManager.Instance.user != null)
            {
                PhotonNetwork.NickName = FirebaseAuthManager.Instance.user.DisplayName;
                changeNicknameInput.placeholder.GetComponent<TMP_Text>().text = FirebaseAuthManager.Instance.user.DisplayName;
            }
            else
            {
                PhotonNetwork.NickName = "Tester" + PhotonNetwork.CountOfPlayers;
                Debug.Log("유저가 없다");
                return;
            }
        }

        
        //    return;
        //if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Leaving)
        //    return;

        if (PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InLobby)
        {
                PhotonNetwork.JoinLobby();
        }
        //260113 최정욱 방 나가고 로비로 돌아올때 마스터서버로 전환 기다리기


        //if (PhotonNetwork.InLobby)
        //    return;
        //    //yield break;
        //if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Leaving)
        //    return;
            //yield break;
        //yield return new WaitUntil(() => PhotonNetwork.InLobby);
        //yield return null;

    }

    //260113 최정욱 방 나가고 로비로 돌아올때 마스터서버로 전환 기다리기
    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.InLobby)
            return;
        PhotonNetwork.JoinLobby();
        Debug.Log("마스터와 커넥트");
    }




    public void CreateRoom()
    {
        //PhotonNetwork.CreateRoom(createRoomInput.text, new RoomOptions { MaxPlayers=4});//옆에 인풋필드에 들어있던 내용의 이름으로 방 생성
        PhotonNetwork.CreateRoom(createRoomInput.text);//옆에 인풋필드에 들어있던 내용의 이름으로 방 생성
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinRoomInput.text);
    }
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }
    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomOrCreateRoom();//만드는 것까지 하거나
    }

    public void ExitLobby()
    {
        PhotonNetwork.LeaveLobby();//로비 떠나라고 포톤에게 지시, 데이터 저장 하려면 씬 이동 후 해당 코드 실행하는 것도 괜찮음
        SceneManager.LoadScene(0);//다시 타이틀 화면으로
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)//룸 정보가 변했을 때, 조인 로비 성공시 한 번,
    {
        foreach (RoomInfo roomInfo in roomList)
        {
            Debug.Log("방 목록 가져오기");
            if (roomInfo.RemovedFromList)
            {
                if (curRoomList.ContainsKey(roomInfo.Name)) 
                {
                    Destroy(curRoomList[roomInfo.Name]);
                    curRoomList.Remove(roomInfo.Name);
                }
                continue;
            }
            if (!curRoomList.ContainsKey(roomInfo.Name))
            {
                GameObject roomBtn = Instantiate(roomPrefab, roomListPanel);
                roomBtn.GetComponentInChildren<TextMeshProUGUI>().text = roomInfo.Name;
                roomBtn.GetComponent<Button>().onClick.AddListener(() => JoinRoom(roomInfo.Name));
                curRoomList.Add(roomInfo.Name, roomBtn);
            }
            
            ////방 재생성 방지
            //if (!curRoomList.Contains(roomInfo.Name))
            //{
            //    var room = Instantiate(roomPrefab, roomListPanel);

            //    room.GetComponentInChildren<TextMeshProUGUI>().text = roomInfo.Name;
            //    room.GetComponent<Button>().onClick.AddListener(() => JoinRoom(roomInfo.Name));
            //    curRoomList.Add(roomInfo.Name);
            //}
        }
    }
    public void Refresh()//룸 새로고침 : 로비 나갔다 들어오기
    {
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방 입장 및 룸 씬으로 전환 요청");
        SceneManager.LoadScene(roomSceneName);
    }


    public void ChangeNickName()
    {
        StartCoroutine(FirebaseAuthManager.Instance.ChangeNickName(changeNicknameInput.text, changeNicknameInput));
    }


    //260113 최정욱
    public void ReturnToLogin()
    {
        FirebaseAuth.DefaultInstance.SignOut();
        SceneManager.LoadScene("Login");

    }

}
