using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class ChattingManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject chatInput;
    TMP_InputField chatInputField;
    [SerializeField] TextMeshProUGUI chattingText;
    string _myName;
    PhotonView pv;
    PlayerInput playerInput;
    bool onChat=false;

    public void Start()
    {
        Debug.Log("방 입장");

        chatInputField = chatInput.GetComponent<TMP_InputField>();
        chattingText.text += "환영합니다 " + _myName + "님.";
        pv= GetComponent<PhotonView>();
        _myName = PhotonNetwork.NickName;
        playerInput = GetComponent<PlayerInput>();

        playerInput.actions["Enter"].performed += SendControl;

    }

    public override void OnLeftRoom()
    {
        playerInput.actions["Enter"].performed -= SendControl;
    }
    private void SendControl(InputAction.CallbackContext context)
    {
        Debug.Log("엔터 입력");
        //if (!pv.IsMine)
        //    return;
        if (!onChat)
        {
            onChat = true;
            chatInput.SetActive(true);
            chatInputField.Select();
            chatInputField.ActivateInputField();
        }
        else
        {
            StartCoroutine(SendMyMessage());
        }
        
    }

    IEnumerator SendMyMessage()
    {
        Debug.Log("채팅입력");
        yield return null;
        onChat = false;
        string message = "\n" + _myName + ": " + chatInputField.text + " ";
        pv.RPC(nameof(SendChat), RpcTarget.All, message);
        chatInputField.text = "";
        chatInput.SetActive(false);
    }

    [PunRPC]
    public void SendChat(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;
        chattingText.text += message;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        chattingText.text += "\n" + newPlayer.NickName + "님이 방에 입장하셨습니다.";
    }

}
