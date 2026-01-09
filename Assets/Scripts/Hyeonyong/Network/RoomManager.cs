using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using System.Collections.Generic;
using System.Collections;
public class RoomManager : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// onJointRoom() : 방 참가 성공
    /// OnJoinRandomFailed() : 랜덤 방 참가 실패(대부분 방이 없음) => 방 생성 필요
    /// OnCreatedRoom() : 방 생성 성공(방장이 됨)
    /// </summary>
    [SerializeField] Button roomBtn;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => PhotonNetwork.InRoom);//방에 입장했는지
        //yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);//방에 연결 및 준비되었는지
        Player[] players = PhotonNetwork.PlayerList;//방 속 사람을 받아옴
        foreach (var p in players)
        {
            Debug.Log("방 안의 사람들 목록: " + p.NickName);
        }
        //roomBtn.interactable = PhotonNetwork.IsMasterClient; //방장이 아니라면 버튼 상호작용 못하도록
        if (PhotonNetwork.IsMasterClient == false)
        {
            roomBtn.interactable = false;
            //혹은 방장이 아니라면 text를 start 대신 ready 등등으로 변경 후 기능도 다르게 연동한다
        }
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient == true)
        {
            PhotonNetwork.LoadLevel("Game");//네트워크 상에서 씬 바꾸는 것
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + "님이 방에 입장하셨습니다.");
    }

    public override void OnPlayerLeftRoom(Player newPlayer)
    {
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
        
    }

    //room property를 이용해 다른 인원의 레디 정보 등 불러올 수 있음
}
