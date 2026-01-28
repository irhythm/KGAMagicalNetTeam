using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using WebSocketSharp;

public class ChattingManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject chatInput;
    [SerializeField] GameObject chattingPanel;
    TMP_InputField chatInputField;
    [SerializeField] TextMeshProUGUI chattingText;
    string _myName;
    PhotonView pv;
    //PlayerInput playerInput;
    [Header("인풋 액션")]
    [SerializeField] private InputActionReference playerInput;
    bool onChat=false;

    [SerializeField] string uiName="Chat";

    [SerializeField] bool onReadyRoom=false;

    Coroutine receiveCoroutine;
    public void Start()
    {
        Debug.Log("방 입장");

        chatInputField = chatInput.GetComponent<TMP_InputField>();
        pv= GetComponent<PhotonView>();
        _myName = PhotonNetwork.NickName;
        //chattingText.text += "환영합니다 " + _myName + "님.";
        //playerInput = GetComponent<PlayerInput>();
        playerInput.action.Enable();
        playerInput.action.performed += SendControl;
        //playerInput.actions["Enter"].performed += SendControl;

    }
    private void OnDisable()
    {
        base.OnDisable();
        playerInput.action.performed -= SendControl;
    }
    public override void OnLeftRoom()
    {
        //playerInput.actions["Enter"].performed -= SendControl;
        playerInput.action.Disable();
    }

    private void SendControl(InputAction.CallbackContext context)
    {
        Debug.Log("엔터 입력");
        //if (!pv.IsMine)
        //    return;
        if (!onChat)
        {
            if (!onReadyRoom)
            {
                UIManager.Instance.OpenUI(uiName);
                chattingPanel.SetActive(true);
            }
            onChat = true;
            chatInput.SetActive(true);
            chatInputField.Select();
            chatInputField.ActivateInputField();
        }
        else
        {
            if (!onReadyRoom)
            {
                UIManager.Instance.CloseUI(uiName);
            }
            StartCoroutine(SendMyMessage());
        }
        
    }

    IEnumerator SendMyMessage()
    {
        Debug.Log("채팅입력");
        //yield return CoroutineManager.WaitForSeconds(0.3f);
        yield return new WaitForSeconds(0.3f);
        onChat = false;
        if (!chatInputField.text.IsNullOrEmpty())
        {
            string message = "\n" + _myName + ": " + chatInputField.text + " ";
            pv.RPC(nameof(SendChat), RpcTarget.All, message);
        }
        chatInputField.text = "";
        chatInput.SetActive(false);

        if (receiveCoroutine == null)
            chattingPanel.SetActive(false);
    }
    [PunRPC]
    public void SendChat(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }
        chattingText.text += message;
        if (!onReadyRoom)
        {
            if(receiveCoroutine != null)
                StopCoroutine(receiveCoroutine);
            receiveCoroutine = StartCoroutine(ReceiveMessage());
        }
    }
    IEnumerator ReceiveMessage()
    {
        chattingPanel.SetActive(true);
        yield return CoroutineManager.waitForSeconds(3f);
        yield return new WaitUntil(() => !onChat);
        chattingPanel.SetActive(false);
        receiveCoroutine = null;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        chattingText.text += "\n" + newPlayer.NickName + "님이 방에 입장하셨습니다.";
    }


    IEnumerator CheckGameManager()
    {
        yield return new WaitUntil(() => FindAnyObjectByType(typeof(GameManager)));
        UIManager.Instance.AddUI(uiName);
    }
}
