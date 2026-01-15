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

    private PlayableCharacter playableCharacter;

    [SerializeField] private PlayerView playerView;


    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] GameObject myInfoPrefab;
    [SerializeField] GameObject magicInfoPrefab;

    [SerializeField] Transform playerInfoPanel;
    [SerializeField] Transform myInfoPanel;
    [SerializeField] Transform magicInfoPanel;
    GameObject playerInfo;
    GameObject magicInfo;

    //[SerializeField] Image speakerImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pv = GetComponent<PhotonView>();
        pvv = GetComponent<PhotonVoiceView>();

        playableCharacter = GetComponent<PlayableCharacter>();

        SubscribeEvents();

        if (pv.IsMine)
        {
            if (myInfoPanel == null)
                myInfoPanel = UIManager.Instance.myInfoPanel;
            playerInfo = Instantiate(myInfoPrefab, myInfoPanel);
            SetMyInfo();
            //magicInfo = Instantiate(magicInfoPrefab, magicInfoPanel);
            //SetMagicInfo();

            playerView.UpdatePlayerHP(1f);
        }
        else
        {
            if (playerInfoPanel == null)
                playerInfoPanel = UIManager.Instance.playerInfoPanel;
            playerInfo = Instantiate(playerInfoPrefab, playerInfoPanel);
            SetPlayerInfo();

            playerView.UpdatePlayerHP(1f);
        }
        SetPlayerName(pv.Owner.NickName);

        testTakeDamageAction.action.Enable();
        testTakeDamageAction.action.performed += TestTakeDamage;

        //playerView.SetVoiceImage(speakerImage);
    }

    // 구독 해제
    private void OnDisable()
    {
        UnsubscribeEvents();
        Destroy(playerInfo);
    }

    // 이벤트 구독 호출
    private void SubscribeEvents()
    {
        if (playableCharacter != null)
        {
            playableCharacter.OnHpChanged += HandleHpChanged;
            // playableCharacter.OnDie += HandleDie; // 이벤트 필요하면 여기에서 연결
        }
    }

    // 구독 해제 호출
    private void UnsubscribeEvents()
    {
        if (playableCharacter != null)
        {
            playableCharacter.OnHpChanged -= HandleHpChanged;
            // playableCharacter.OnDie -= HandleDie; // 죽었을 때 뭐 할거면 구현해야할 듯?
        }
    }

    // 업데이트시 호출
    private void HandleHpChanged(float hpRatio)
    {
        playerView.UpdatePlayerHP(hpRatio);
    }


    public void TestTakeDamage(InputAction.CallbackContext context)
    {
        if (!pv.IsMine)
            return;

        TakeDamage(10f);
        //pv.RPC(nameof(OnTakeDamageRPC), RpcTarget.All, 10f);
        //pv.RPC(nameof(UsingMagic),RpcTarget.All);

    }

    public void TakeDamage(float takeDamage)
    {
        if (!pv.IsMine)
            return;

        playableCharacter.OnAttacked(takeDamage);

        // pv.RPC(nameof(OnTakeDamageRPC), RpcTarget.All, 10f);
    }


    public void TakeDamageByMagic(float takeDamage)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("FriendlyFire", out object isCheck))
        {
            if ((bool)isCheck)
            {
                playableCharacter.OnAttacked(takeDamage);
                // pv.RPC(nameof(OnTakeDamageRPC), RpcTarget.All, takeDamage);
            }
        }
    }


    public void SetPlayerInfo()
    {
        playerView.SetPlayerInfo(
playerInfo.transform.GetChild(0).GetComponent<TextMeshProUGUI>(),
playerInfo.transform.GetChild(1).GetComponent<Image>(),
playerInfo.transform.GetChild(2).GetComponent<Image>()
);
    }
    public void SetMyInfo()
    {
        playerView.SetMyInfo(
playerInfo.transform.GetChild(0).GetComponent<TextMeshProUGUI>(),
playerInfo.transform.GetChild(1).GetComponent<Image>()
);
    }


    //public void SetMagicOnHand()
    public void SetMagicInfo()
    {
        //크기가 정해져 있어서 리스트나 큐 대신 배열 사용
        int count = magicInfo.transform.GetChild(2).childCount;
        GameObject[] magic = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            magic[i] = magicInfo.transform.GetChild(2).GetChild(i).gameObject;
        }
        playerView.SetMagicInfo(magic);
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

    // 이벤트로 바꿔서 호출 이제 필요없을 듯?
    public void UpdatePlayerHp()
    {
        // playerView.UpdatePlayerHP(playerModel.CurHp / playerModel.MaxHp);
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
        if (!pv.IsMine)
        {
            playerView.CheckVoiceImage(pvv.IsSpeaking);
        }
    }

    //public void UsingMagic(MagicDataSO magicData)
    //{
    //    playerView.SetMagicIcon(magicData.itemImage, magicData.cooldown);
    //}


}