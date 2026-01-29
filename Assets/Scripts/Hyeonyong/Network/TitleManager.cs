using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
public class TitleManager : MonoBehaviourPunCallbacks
{
    [SerializeField] string roomSceneName;
    [SerializeField] bool onTest = false;
    [SerializeField] AudioClip titleAudio;
    private void Awake()
    {
        //호스트가 씬 이동시 클라이언트도 같이 이동
       PhotonNetwork.AutomaticallySyncScene=true;
    }

    private void Start()
    {
        SoundManager.Instance.PlayBGM(titleAudio);
    }

    //버튼 연결을 위해 우리가 만든 메서드임 OnConnectToServer아님
    public void ConnectToServer()
    {
        if (PhotonNetwork.IsConnected)
        {
            OnConnectedToMaster();
        }
        else
        {
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "kr";
            PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        if (onTest)
        {
            PhotonNetwork.JoinRandomOrCreateRoom();
        }
        else
        {
            SceneManager.LoadScene("Login");
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방 입장 및 룸 씬으로 전환 요청");
        SceneManager.LoadScene(roomSceneName);
    }
}
