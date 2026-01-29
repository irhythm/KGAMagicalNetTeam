using Photon.Pun;
using System.Collections;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PanController : MonoBehaviourPunCallbacks, IMagicInteractable, IDamageable
{
    [SerializeField] GameObject PanHpBanner;
    [SerializeField] float fryingPanMaxHp = 20;
    PhotonView pv;
    PanView panView;

    Renderer myRenderer;
    Material myMaterial;

    Color originColor;
    [SerializeField] Color damageColor;
    [SerializeField] float damageChangeDelay=1f;

    Coroutine damageCoroutine_Color;
    Coroutine damageCoroutine_Noise;
    [SerializeField] float noiseStrength = 3f;
    [SerializeField] float noiseDelay = 0.01f;

    Vector3 originPos;
    
    private void Start()
    {
        pv= GetComponent<PhotonView>();
        panView = GetComponent<PanView>();


        myRenderer = GetComponent<Renderer>();
        myMaterial = myRenderer.material;
        myMaterial.EnableKeyword("_EMISSION");
        originColor = myMaterial.GetColor("_EmissionColor");
        originPos=transform.position;

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

    public void TakeDamage(float damage)
    {
        pv.RPC(nameof(TakeDamageRPC), RpcTarget.All, damage);
    }

    [PunRPC]
    void TakeDamageRPC(float damage)
    {
        float curHp = GetFryingPanHP()- damage;
        //HandleHpChanged(curHp/fryingPanMaxHp);
        SetFryingPanHP(curHp);
        //CheckDie();

        if (!gameObject.activeSelf)
            return;
        if (damageCoroutine_Color != null)
            StopCoroutine(damageCoroutine_Color);
        damageCoroutine_Color = StartCoroutine(TakeDamageEvent_Color());

        if (damageCoroutine_Noise != null)
            StopCoroutine(damageCoroutine_Noise);
        damageCoroutine_Noise = StartCoroutine(TakeDamageEvent_Noise());
    }

    //룸 프로퍼티의 값이 바뀌면 호출
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        // 바뀐 프로퍼티 중에 "HP_1"이 포함되어 있는지 확인
        if (propertiesThatChanged.ContainsKey(NetworkProperties.FRYINGPANHP))
        {
            HandleHpChanged(GetFryingPanHP() / fryingPanMaxHp);
            CheckDie();
        }
    }
    void CheckDie()
    {
        if (GetFryingPanHP() <= 0)
        {
            if (PanHpBanner != null)
                PanHpBanner.SetActive(false);
            if (gameObject != null)
            {
                gameObject.SetActive(false);
                return;
            }
        }
    }

    void SetFryingPanHP(float curHp)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.FRYINGPANHP, curHp);
        }
    }
    private void OnDisable()
    {
        base.OnDisable();
        //비활성화시 실행할 코드
        if(damageCoroutine_Color!=null)
            StopCoroutine(damageCoroutine_Color);
        if(damageCoroutine_Noise!=null)
            StopCoroutine(damageCoroutine_Noise);
    }

    float GetFryingPanHP()
    {
        return PhotonNetwork.CurrentRoom.GetProps<float>(NetworkProperties.FRYINGPANHP);
    }

    private void HandleHpChanged(float hpRatio)
    {
        panView.UpdateHp(hpRatio);
    }

    IEnumerator TakeDamageEvent_Color()
    {
        myMaterial.SetColor("_EmissionColor", damageColor);
        yield return CoroutineManager.waitForSeconds(damageChangeDelay);
        myMaterial.SetColor("_EmissionColor", originColor);
        damageCoroutine_Color = null;
    }

    IEnumerator TakeDamageEvent_Noise()
    {
        float curTime = damageChangeDelay;
        float curNoiseStrength = noiseStrength;
        float ratio = curTime;
        while (curTime > 0)
        {
            ratio = curTime / damageChangeDelay;
            //curNoiseStrength = Mathf.Lerp(curNoiseStrength, 0, ratio);
            float x = Random.Range(-1f, 1f) * curNoiseStrength;
            float y = Random.Range(-1f, 1f) * curNoiseStrength;

            transform.position = new Vector3(originPos.x + x, originPos.y + y, originPos.z);
            curTime-=noiseDelay;
            yield return CoroutineManager.waitForSeconds(noiseDelay);
        }
        transform.position = originPos;
        damageCoroutine_Noise = null;
    }

    private void OnDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
            InitFryingPan();
    }

    public void OnMagicInteract(GameObject magic, MagicDataSO data, int attackerActorNr)
    {
        switch(data.magicType)
        {
            case MagicType.Fireball:
                FireballReaction(magic.transform.position, data, attackerActorNr);
                break;
            default:
                Debug.LogWarning("[PanController] 마법 타입 설정 안했거나 구현을 안했음");
                break;
        }
    }

    public void FireballReaction(Vector3 explosionPos, MagicDataSO data, int attackerActorNr)
    {
        if (pv.IsMine)
        {
            TakeDamage(data.damage);
        }
    }
}
