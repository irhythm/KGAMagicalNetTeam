using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
public class ServerConnector : MonoBehaviourPunCallbacks
{
    private void Awake()
    {
        //호스트가 씬 이동시 클라이언트도 같이 이동
       PhotonNetwork.AutomaticallySyncScene=true;
    }

    //버튼 연결을 위해 우리가 만든 메서드임 OnConnectToServer아님
    public void ConnectToServer()
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "kr";
        PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("마스터 서버 연동 성공");
       // PhotonNetwork.JoinLobby();//로비 입장을 시키는 명령
        SceneManager.LoadScene("Lobby");//싱글톤도 아니고 dontdestroy 아니면 씬 넘어갈 경우 해당 스크립트 파괴
    }
    //public override void OnJoinedLobby()
    //{
    //    Debug.Log("로비 입장 완료");
    //}
}
