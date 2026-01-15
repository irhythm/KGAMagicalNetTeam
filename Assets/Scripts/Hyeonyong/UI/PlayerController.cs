using Photon.Pun;
using Photon.Voice.PUN;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPun
{
    [SerializeField] private InputActionReference testTakeDamageAction;
    PhotonView pv;
    PhotonVoiceView pvv;
    Recorder recorder;
    [SerializeField] float maxHp = 100f;
    public PlayerModel playerModel;
    [SerializeField] private PlayerView playerView;


    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] GameObject myInfoPrefab;

    [SerializeField] Transform playerInfoPanel;
    [SerializeField] Transform myInfoPanel;
    GameObject playerInfo;

    //[SerializeField] Image speakerImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pv = GetComponent<PhotonView>();
        pvv = GetComponent<PhotonVoiceView>();

        //if (!pv.IsMine)
        //    return;
        playerModel = new PlayerModel(maxHp);
        playerModel.Init();


        if (pv.IsMine)
        {
            if (myInfoPanel == null)
                myInfoPanel = UIManager.Instance.myInfoPanel;
            playerInfo = Instantiate(myInfoPrefab, myInfoPanel);
            SetMyInfo(
playerInfo.transform.GetChild(0).GetComponent<TextMeshProUGUI>(),
playerInfo.transform.GetChild(1).GetComponent<Image>()
);
        }
        else
        {
            if(playerInfoPanel==null)
                playerInfoPanel = UIManager.Instance.playerInfoPanel;
            playerInfo = Instantiate(playerInfoPrefab, playerInfoPanel);
            SetPlayerInfo(
        playerInfo.transform.GetChild(0).GetComponent<TextMeshProUGUI>(),
        playerInfo.transform.GetChild(1).GetComponent<Image>(),
        playerInfo.transform.GetChild(2).GetComponent<Image>()
        );
        }
        SetPlayerName(pv.Owner.NickName);

        testTakeDamageAction.action.Enable();
        testTakeDamageAction.action.performed += TestTakeDamage;

        //playerView.SetVoiceImage(speakerImage);
    }

    public void TestTakeDamage(InputAction.CallbackContext context)
    {
        if (!pv.IsMine)
            return;

        pv.RPC(nameof(OnTakeDamageRPC), RpcTarget.All, 10f);

    }

    public void TakeDamage(float takeDamage)
    {
        if (!pv.IsMine)
            return;
        pv.RPC(nameof(OnTakeDamageRPC), RpcTarget.All, 10f);
    }
    private void OnDisable()
    {
        Destroy(playerInfo);
    }

    [PunRPC]
    public void OnTakeDamageRPC(float takeDamage)
    {
        bool isDie = false;
        isDie = playerModel.TakeDamage(takeDamage);
        playerView.UpdatePlayerHP(playerModel.CurHp / playerModel.MaxHp);
        if (isDie)
        {
            Debug.Log("캐릭터 사망");
        }
    }

    public void SetPlayerInfo(TextMeshProUGUI name, Image hp, Image speaker)
    {
        playerView.SetPlayerInfo(name, hp, speaker);
    }
    public void SetMyInfo(TextMeshProUGUI name, Image hp)
    {
        playerView.SetMyInfo(name, hp);
    }
    public void CheckPlayerName(TextMeshProUGUI name)
    {
        if (name == null)
        {
            Debug.Log("이름 없음");
        }
        else
        {
            Debug.Log("이름 있음 : " + name.text);
        }
    }
    public void CheckPlayerImage(Image hp)
    {
        if (hp == null)
        {
            Debug.Log("이미지 없음");
        }
        else
        {
            Debug.Log("이미지 있음 : " + hp.fillAmount);
        }
    }
    public void UpdatePlayerHp()
    {
        playerView.UpdatePlayerHP(playerModel.CurHp / playerModel.MaxHp);
    }

    public void SetPlayerName(string name)
    {
        playerView.SetPlayerName(name);
    }

    void Update()
    {
        //if (pv.IsMine)
        //{
        //    playerView.CheckVoiceImage(pvv.IsRecording);
        //}
        //else
        if(!pv.IsMine)
        {
            playerView.CheckVoiceImage(pvv.IsSpeaking);
        }
    }
}
