using Photon.Pun;
using Photon.Voice.PUN;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPun, IDamageable, IExplosion
{
    PhotonView pv;
    PhotonVoiceView pvv;

    private PlayableCharacter playableCharacter;
    public PlayerView playerView;

    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] GameObject myInfoPrefab;
    [SerializeField] GameObject magicInfoPrefab;

    [SerializeField] Transform playerInfoPanel;
    [SerializeField] Transform myInfoPanel;
    [SerializeField] Transform magicInfoPanel;

    GameObject playerInfo;
    GameObject magicInfo;

    IEnumerator Start()
    {
        pv = GetComponent<PhotonView>();
        pvv = GetComponent<PhotonVoiceView>();
        playableCharacter = GetComponent<PlayableCharacter>();

        SubscribeEvents();

        if (pv.IsMine)
        {
            if (myInfoPanel == null) 
                myInfoPanel = UIManager.Instance.myInfoPanel;

            if (magicInfoPanel == null) 
                magicInfoPanel = UIManager.Instance.IconPanel;

            playerInfo = Instantiate(myInfoPrefab, myInfoPanel);
            SetMyInfo();

            magicInfo = Instantiate(magicInfoPrefab, magicInfoPanel);
            SetMagicInfo();
            SetHandInfo();

            playerView.UpdatePlayerHP(1f);

            PlayerMagicSystem magicSystem = GetComponent<PlayerMagicSystem>();
            if (magicSystem != null)
            {
                playerView.BindMagicSystem(magicSystem, playableCharacter);
            }
        }
        else
        {
            if (playerInfoPanel == null) 
                playerInfoPanel = UIManager.Instance.playerInfoPanel;

            playerInfo = Instantiate(playerInfoPrefab, playerInfoPanel);
            SetPlayerInfo();
            playerView.UpdatePlayerHP(1f);
        }

        //본래는 로그인 후 닉네임을 가져와야 하지만 테스트 단계이므로 대충 null로 넘김
        //yield return new WaitUntil(()=>pv.Owner !=null);
        yield return null;
        SetPlayerName(pv.Owner.NickName);

        RoomManager.Instance?.fryingPanLogic.AddTarget(transform);
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
        if (playerInfo != null) 
            Destroy(playerInfo);
    }

    private void SubscribeEvents()
    {
        if (playableCharacter != null) 
            playableCharacter.OnHpChanged += HandleHpChanged;
    }

    private void UnsubscribeEvents()
    {
        if (playableCharacter != null) 
            playableCharacter.OnHpChanged -= HandleHpChanged;
        testTakeDamageAction.action.performed -= TestTakeDamage;
    }

    private void HandleHpChanged(float hpRatio)
    {
        playerView.UpdatePlayerHP(hpRatio);
    }

    public void OnExplosion(Vector3 explosionPos, MagicDataSO data, int attackerActorNr)
    {
        if (pv.OwnerActorNr == attackerActorNr) return;
        if (!IsFriendlyFireOn()) return;

        if (playableCharacter.Rigidbody != null)
        {
            playableCharacter.Rigidbody.AddExplosionForce(data.knockbackForce, explosionPos, data.radius, data.forceUpward, ForceMode.Impulse);
        }

        if (pv.IsMine)
        {
            TakeDamage(data.damage);
        }
    }

    public void TakeDamage(float takeDamage)
    {
        if (!pv.IsMine) return;
        playableCharacter.OnAttacked(takeDamage);
    }

    private bool IsFriendlyFireOn()
    {
        if (PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("FriendlyFire", out object isFF))
        {
            return (bool)isFF;
        }
        return false;
    }


    public void SetPlayerInfo()
    {
        playerView.SetPlayerInfo(
            playerInfo.transform.GetChild(0).GetComponent<TextMeshProUGUI>(),
            playerInfo.transform.GetChild(2).GetComponent<Image>(),
            playerInfo.transform.GetChild(3).GetComponent<Image>()
        );
    }
    public void SetMyInfo()
    {
        playerView.SetMyInfo(
            playerInfo.transform.GetChild(0).GetComponent<TextMeshProUGUI>(),
            playerInfo.transform.GetChild(2).GetComponent<Image>()
        );
    }

    public void SetMagicInfo()
    {
        int count = magicInfo.transform.GetChild(2).childCount;
        GameObject[] magic = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            magic[i] = magicInfo.transform.GetChild(2).GetChild(i).gameObject;
        }
        playerView.SetMagicInfo(magic);
    }

    public void SetHandInfo()
    {
        GameObject[] hand = new GameObject[2];
        hand[0] = magicInfo.transform.GetChild(0).gameObject;
        hand[1] = magicInfo.transform.GetChild(1).gameObject;
        playerView.SetMagicInfoOnHand(hand);
    }

    public void SetPlayerName(string name)
    {
        playerView.SetPlayerName(name);
    }

    void Update()
    {
        if (!pv.IsMine)
        {
            playerView.CheckVoiceImage(pvv.IsSpeaking);
        }
        //else
        //{
        //    playerView.CheckVoiceImage(pvv.IsRecording);
        //}
    }
}
#region 레거시 코드
//using Photon.Pun;
//using Photon.Voice.PUN;
//using TMPro;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.Profiling;
//using UnityEngine.UI;

//public class PlayerController : MonoBehaviourPun, IDamageable
//{
//    [SerializeField] private InputActionReference testTakeDamageAction;
//    PhotonView pv;
//    PhotonVoiceView pvv;
//    Recorder recorder;

//    private PlayableCharacter playableCharacter;

//    public PlayerView playerView;


//    [SerializeField] GameObject playerInfoPrefab;
//    [SerializeField] GameObject myInfoPrefab;
//    [SerializeField] GameObject magicInfoPrefab;

//    [SerializeField] Transform playerInfoPanel;
//    [SerializeField] Transform myInfoPanel;
//    [SerializeField] Transform magicInfoPanel;
//    GameObject playerInfo;
//    GameObject magicInfo;

//    //[SerializeField] Image speakerImage;
//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        pv = GetComponent<PhotonView>();
//        pvv = GetComponent<PhotonVoiceView>();

//        playableCharacter = GetComponent<PlayableCharacter>();

//        SubscribeEvents();

//        if (pv.IsMine)
//        {
//            if (myInfoPanel == null)
//                myInfoPanel = UIManager.Instance.myInfoPanel;
//            if(magicInfoPanel == null)
//                magicInfoPanel = UIManager.Instance.IconPanel;
//            playerInfo = Instantiate(myInfoPrefab, myInfoPanel);
//            SetMyInfo();
//            magicInfo = Instantiate(magicInfoPrefab, magicInfoPanel);
//            SetMagicInfo();
//            SetHandInfo();

//            playerView.UpdatePlayerHP(1f);
//        }
//        else
//        {
//            if (playerInfoPanel == null)
//                playerInfoPanel = UIManager.Instance.playerInfoPanel;
//            playerInfo = Instantiate(playerInfoPrefab, playerInfoPanel);
//            SetPlayerInfo();

//            playerView.UpdatePlayerHP(1f);
//        }
//        SetPlayerName(pv.Owner.NickName);

//        testTakeDamageAction.action.Enable();
//        testTakeDamageAction.action.performed += TestTakeDamage;

//        //playerView.SetVoiceImage(speakerImage);
//    }

//    // 구독 해제
//    private void OnDisable()
//    {
//        UnsubscribeEvents();
//        Destroy(playerInfo);
//    }

//    // 이벤트 구독 호출
//    private void SubscribeEvents()
//    {
//        if (playableCharacter != null)
//        {
//            playableCharacter.OnHpChanged += HandleHpChanged;
//            // playableCharacter.OnDie += HandleDie; // 이벤트 필요하면 여기에서 연결
//        }
//    }

//    // 구독 해제 호출
//    private void UnsubscribeEvents()
//    {
//        if (playableCharacter != null)
//        {
//            playableCharacter.OnHpChanged -= HandleHpChanged;
//            // playableCharacter.OnDie -= HandleDie; // 죽었을 때 뭐 할거면 구현해야할 듯?
//        }
//    }

//    // 업데이트시 호출
//    private void HandleHpChanged(float hpRatio)
//    {
//        playerView.UpdatePlayerHP(hpRatio);
//    }


//    public void TestTakeDamage(InputAction.CallbackContext context)
//    {
//        if (!pv.IsMine)
//            return;

//        TakeDamage(10f);
//        //pv.RPC(nameof(OnTakeDamageRPC), RpcTarget.All, 10f);
//        //pv.RPC(nameof(UsingMagic),RpcTarget.All);

//    }

//    public void TakeDamage(float takeDamage)
//    {
//        if (!pv.IsMine)
//            return;

//        playableCharacter.OnAttacked(takeDamage);

//        // pv.RPC(nameof(OnTakeDamageRPC), RpcTarget.All, 10f);
//    }


//    public void SetPlayerInfo()
//    {
//        playerView.SetPlayerInfo(
//playerInfo.transform.GetChild(0).GetComponent<TextMeshProUGUI>(),
//playerInfo.transform.GetChild(1).GetComponent<Image>(),
//playerInfo.transform.GetChild(2).GetComponent<Image>()
//);
//    }
//    public void SetMyInfo()
//    {
//        playerView.SetMyInfo(
//playerInfo.transform.GetChild(0).GetComponent<TextMeshProUGUI>(),
//playerInfo.transform.GetChild(1).GetComponent<Image>()
//);
//    }


//    public void SetMagicInfo()
//    {
//        //크기가 정해져 있어서 리스트나 큐 대신 배열 사용
//        int count = magicInfo.transform.GetChild(2).childCount;
//        GameObject[] magic = new GameObject[count];
//        for (int i = 0; i < count; i++)
//        {
//            magic[i] = magicInfo.transform.GetChild(2).GetChild(i).gameObject;
//        }
//        playerView.SetMagicInfo(magic);
//    }

//    public void SetHandInfo()
//    {
//        GameObject[] hand = new GameObject[2];
//        hand[0] = magicInfo.transform.GetChild(0).gameObject;
//        hand[1] = magicInfo.transform.GetChild(1).gameObject;
//        playerView.SetMagicInfoOnHand(hand);
//    }

//    public void CheckPlayerName(TextMeshProUGUI name)
//    {
//        if (name == null)
//        {
//            Debug.Log("이름 없음");
//        }
//        else
//        {
//            Debug.Log("이름 있음 : " + name.text);
//        }
//    }
//    public void CheckPlayerImage(Image hp)
//    {
//        if (hp == null)
//        {
//            Debug.Log("이미지 없음");
//        }
//        else
//        {
//            Debug.Log("이미지 있음 : " + hp.fillAmount);
//        }
//    }

//    // 이벤트로 바꿔서 호출 이제 필요없을 듯?
//    public void UpdatePlayerHp()
//    {
//        // playerView.UpdatePlayerHP(playerModel.CurHp / playerModel.MaxHp);
//    }

//    public void SetPlayerName(string name)
//    {
//        playerView.SetPlayerName(name);
//    }

//    void Update()
//    {
//        if (!pv.IsMine)
//        {
//            playerView.CheckVoiceImage(pvv.IsSpeaking);
//        }
//    }

//    //public void UsingMagic(MagicDataSO magicData)
//    //{
//    //    playerView.SetMagicIcon(magicData.itemImage, magicData.cooldown);
//    //}

//    public void SetItem(InventoryDataSO item, bool isLeft)
//    {
//        playerView.SetIconOnHand(item, isLeft);
//    }

//    public void SetCoolTime(MagicBase magic, bool isLeft)
//    {
//        playerView.CheckCoolTimeOnHand(magic, isLeft);
//    }


//    public void SetMagicIcon(InventoryDataSO data, PlayableCharacter player)
//    {
//        if(data == null)
//            return;

//        MagicDataSO magicData = data as MagicDataSO;
//        if(magicData == null) 
//            return;

//        MagicBase targetLogic = player.Inventory.GetMagicInstance(magicData);

//        if (targetLogic == null)
//        {
//            targetLogic = magicData.CreateInstance();
//        }
//        if (targetLogic.CurrentCooldown <= 0)
//            return;
//        playerView.SetMagicIcon(targetLogic);

//    }

//}
#endregion