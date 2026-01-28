using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;


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
            StartCoroutine(CoLoadRoom()); 
        }
    }

    IEnumerator CoLoadRoom()
    {
        PhotonNetwork.IsMessageQueueRunning = false;
        yield return new WaitForSeconds(1.0f);
        PhotonNetwork.LoadLevel("Room_new");
    }


    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            backToRoomBtn.gameObject.SetActive(true);
            PhotonNetwork.DestroyAll();
        }
        else
        {
            backToRoomBtn.gameObject.SetActive(false);
        }
    }
}
