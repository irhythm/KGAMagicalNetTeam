using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using System.Collections;
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    [SerializeField] GameObject playerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        //방에 들어가지 않은 상태로 수행한 상태로 들어와서 이미 들어와있는 사람은 모름
        PlayerManager.LocalPlayerInstance = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0, 5, 0), Quaternion.identity, 0);
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

    // Update is called once per frame
    void Update()
    {
        
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

}
