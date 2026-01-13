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
    [SerializeField] TMP_InputField createRoomInput;
    [SerializeField] TMP_InputField joinRoomInput;
    [SerializeField] TMP_InputField changeNicknameInput;

    [SerializeField] GameObject roomPrefab;
    [SerializeField] Transform roomListPanel;
    //[SerializeField] List<string> curRoomList = new List<string>();
    [SerializeField] Dictionary<string,GameObject> curRoomList = new Dictionary<string, GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Debug.Log("로비 씬 시작");
        FirebaseAuthManager.Instance.RefreshUser();
        PhotonNetwork.NickName = FirebaseAuthManager.Instance.user.DisplayName;
        if (FirebaseAuthManager.Instance.user == null)
        {
            Debug.Log("유저가 없다");
            //yield break;
            return;
        }    
        changeNicknameInput.placeholder.GetComponent<TMP_Text>().text = FirebaseAuthManager.Instance.user.DisplayName;

        if (!PhotonNetwork.InLobby)
            return;
            //yield break;
        if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Leaving)
            return;
            //yield break;
        PhotonNetwork.JoinLobby();
        //yield return new WaitUntil(() => PhotonNetwork.InLobby);
        //yield return null;

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
        SceneManager.LoadScene("Room");
    }


    public void ChangeNickName()
    {
        StartCoroutine(FirebaseAuthManager.Instance.ChangeNickName(changeNicknameInput.text, changeNicknameInput));
    }

}
