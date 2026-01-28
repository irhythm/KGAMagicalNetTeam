using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class FinalRoundManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject backToRoomBtn;
    [SerializeField] AudioClip loseOrWinAudio;
    private void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.DestroyAll();
    }
    private void Start()
    {
        SoundManager.Instance.PlayBGM(loseOrWinAudio);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (!PhotonNetwork.IsMasterClient)
        {
            backToRoomBtn.gameObject.SetActive(false);
        }
    }
    public void BackToRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Room_new");//네트워크 상에서 씬 바꾸는 것
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            backToRoomBtn.gameObject.SetActive(true);
        }
        else
        {
            backToRoomBtn.gameObject.SetActive(false);
        }
    }
}
