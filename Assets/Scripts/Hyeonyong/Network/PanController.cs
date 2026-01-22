using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PanController : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject PanHpBanner;
    Hashtable roomTable = new Hashtable();
    [SerializeField] float fryingPanMaxHp = 20;
    PhotonView pv;
    PanView panView;

    private void Start()
    {
        pv= GetComponent<PhotonView>();
        panView = GetComponent<PanView>();
        InitFryingPan();
    }
    void InitFryingPan()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SetFryingPanHP(fryingPanMaxHp);
        }
        else
        {
            HandleHpChanged(GetFryingPanHP() / fryingPanMaxHp);
            CheckDie();
        }
            //CheckDie();
    }

    public void OnTakeDamage(float damage)
    {
        pv.RPC(nameof(TakeDamageRPC), RpcTarget.All, damage);
    }

    [PunRPC]
    void TakeDamageRPC(float damage)
    {
        float curHp = GetFryingPanHP()- damage;
        HandleHpChanged(curHp/fryingPanMaxHp);
        SetFryingPanHP(curHp);
        //CheckDie();
    }

    //룸 프로퍼티의 값이 바뀌면 호출
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        // 바뀐 프로퍼티 중에 "HP_1"이 포함되어 있는지 확인
        if (propertiesThatChanged.ContainsKey("FryingPan"))
        {
            HandleHpChanged(GetFryingPanHP() / fryingPanMaxHp);
            CheckDie();
        }
    }
    void CheckDie()
    {
        if (GetFryingPanHP() > 0)
            return;
         
        PanHpBanner.SetActive(false);
        gameObject.SetActive(false);
    }

    void SetFryingPanHP(float curHp)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            roomTable["FryingPan"] = curHp;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomTable);
        }
    }
    private void OnDisable()
    {
        //비활성화시 실행할 코드
    }

    float GetFryingPanHP()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("FryingPan", out object fryingPanHp))
        { 
            return (float) fryingPanHp;
        }
        else
        {
            Debug.Log("값이 없다");
            return -1;
        }
    }

    private void HandleHpChanged(float hpRatio)
    {
        panView.UpdateHp(hpRatio);
    }

}
