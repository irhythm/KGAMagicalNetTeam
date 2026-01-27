using Photon.Pun;
using System.Collections;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PanController : MonoBehaviourPunCallbacks, IExplosion, IDamageable
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
        InitFryingPan();

        myRenderer = GetComponent<Renderer>();
        myMaterial = myRenderer.material;
        myMaterial.EnableKeyword("_EMISSION");
        originColor = myMaterial.GetColor("_EmissionColor");
        originPos=transform.position;
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

    public void OnExplosion(Vector3 explosionPos, MagicDataSO data, int attackerActorNr)
    {
        if (pv.IsMine)
        {
            TakeDamage(data.damage);
        }
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
        Debug.Log("프라이팬 : 현재 HP: " + curHp);
        SetFryingPanHP(curHp);
        //CheckDie();
        if(damageCoroutine_Color!=null) 
            StopCoroutine(damageCoroutine_Color);
        damageCoroutine_Color=StartCoroutine(TakeDamageEvent_Color());

        if(damageCoroutine_Noise != null)
            StopCoroutine(damageCoroutine_Noise);
        damageCoroutine_Noise=StartCoroutine(TakeDamageEvent_Noise());
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
        Debug.Log("프라이팬 : 죽음 체크");
        if (GetFryingPanHP() > 0)
            return;
        Debug.Log("프라이팬 : 죽음");
        if(PanHpBanner != null)
            PanHpBanner.SetActive(false);
        if (gameObject != null)
        {
            gameObject.SetActive(false);
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
        //비활성화시 실행할 코드
        if(damageCoroutine_Color!=null)
            StopCoroutine(damageCoroutine_Color);
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
            Debug.Log("노이즈 발생");
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
}
