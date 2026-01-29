using BzKovSoft.RagdollTemplate.Scripts.Charachter;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(BzRagdoll))]
public class HumanoidRagdollController : MonoBehaviourPun, IMagicInteractable
{
    [Header("컴포넌트부착")]
    [SerializeField] private BzRagdoll bzRagdoll;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;

    [Header("설정값")]
    [SerializeField] private float knockDownDuration = 1.5f; //최소 기절 시간
    [SerializeField] private float getUpAnimationDuration = 2.5f; //일어나는 애니메이션 길이

    //BaseAI 연결 일단 얘도 임시긴함
    [SerializeField] private BaseAI baseAI;

    //상태 관리용
    private bool isRagdollActive = false;
    private float ragdollStartTime;

    //중복기상방지

    private bool isRecovering = false;



    private void Awake()
    {
        bzRagdoll = GetComponent<BzRagdoll>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        baseAI = GetComponent<BaseAI>();
    }

    private void Update()
    {
        if (baseAI != null && baseAI.CurrentHP <= 0) return;

        if (isRecovering) return;

        //일단 대기
        if (Time.time - ragdollStartTime < knockDownDuration) return;

        //방장이 NPC의 기상 타이밍 결정
        //안 그러면 각자 화면에서 따로 일어나서 위치가 꼬임
        if (PhotonNetwork.IsMasterClient && isRagdollActive)
        {
            CheckGetUpCondition();
        }
    }

    //외부용 피격 메서드
    public void ApplyRagdoll(Vector3 force)
    {
        if (isRagdollActive) return; 
        //이미 다운된 상태면 무시 (누워있어도 계속 날아가게 처리?? 일단 보류)
        //무콤 필요하면 주석해제 하고 테스트

        photonView.RPC(nameof(RpcActivateRagdoll), RpcTarget.All, force);
    }
     
    //피격 RPC
    [PunRPC]
    private void RpcActivateRagdoll(Vector3 force)
    {
        isRagdollActive = true;
        isRecovering = false;
        ragdollStartTime = Time.time;

        if (baseAI != null) baseAI.IsKnockedDown = true;

        //에셋 래그돌 시스템<- 어댑터 있음
        bzRagdoll.IsRagdolled = true;

        //위치값 가져와 물리 적용
        //무게중심인 골반에 힘 가하기(추후 부위별 타격? 근데 문제가 많음)
        Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        if (hips != null)
        {
            Rigidbody hipsRigid = hips.GetComponent<Rigidbody>();

            if (hipsRigid != null)
            {
            //임펄스가 기획의도 상 베스트일 듯
            hipsRigid.AddForce(force, ForceMode.Impulse);
            }
            else
            {
                Debug.Log("허리에 리짓바디 없음");

            }
        }
    }


    //기상 체크(방장용)
    private void CheckGetUpCondition()
    {
        if (isRecovering) return;

        //일단 3초
        if (Time.time - ragdollStartTime < knockDownDuration) return;

        Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        if (hips == null)
        {
            Debug.Log("테스트 겟본트랜스폼 없음, 나중에 로그 제거"); 
            return; //방어 코드
        }
        Rigidbody hipsRigid = hips.GetComponent<Rigidbody>();
        if (hipsRigid == null)
        {
            Debug.LogError("골반 리짓바디없읆.,,,,");
            return;
        }

        if (hipsRigid.linearVelocity.magnitude < 0.5f)
        {
            //위치보정, NavMesh 위 유효 좌표를 찾음
            Vector3 getUpPos = hips.position;
            if (NavMesh.SamplePosition(getUpPos, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                getUpPos = hit.position;
            }
            
            //일어나는 중
            isRecovering = true;

            //안전한 위치를 모두에게 전송하며 기상 명령(텔포 좀 할 듯)?
            photonView.RPC(nameof(RpcGetUp), RpcTarget.All, getUpPos);
        }

    }

    //기상실행 RPC -> 애니메이션 시간에 맞춰 코루틴으로?
    [PunRPC]
    private void RpcGetUp(Vector3 syncPosition)
    {
        if (baseAI != null && baseAI.CurrentHP <= 0) return;
        StartCoroutine(CoGetUpProcess(syncPosition));
    }

    private IEnumerator CoGetUpProcess(Vector3 pos)
    {
        //래그돌 해제 -> 기상 애니메이션 블렌딩 시작
        bzRagdoll.IsRagdolled = false;

        //애니메이션 재생 대기
        yield return CoroutineManager.waitForSeconds(getUpAnimationDuration);

        if (animator != null)
        {
            animator.enabled = true;
        }

        if ((agent != null))
        {
            agent.Warp(pos);//위치재설정
            agent.enabled = true;
        }

        //시민 경비 리커버리 시 행동 수행(방장만)
        if (PhotonNetwork.IsMasterClient)
        {
            baseAI.OnRecoverFromKnockdown();
        }
        else
        {
            baseAI.IsKnockedDown = false;
        } 
        isRagdollActive = false;
    }

    public void OnMagicInteract(GameObject magic, MagicDataSO data, int attackerActorNr)
    {
        switch(data.magicType)
        {
            case MagicType.Fireball:
                FireballReaction(magic, data, attackerActorNr);
                break;
            case MagicType.Lightning:
                LightningStrikeReaction(magic, data, attackerActorNr);
                break;
            case MagicType.Tornado:
                TornadoReaction();
                break;
            default:
                Debug.LogWarning("[HumanoidRagdollController] 마법 타입 설정 안했거나 구현을 안했음");
                break;
        }
    }

    public void FireballReaction(GameObject magic, MagicDataSO data, int attackerActorNr)
    {
        Vector3 dir = (transform.position - magic.transform.position).normalized;
        Vector3 force = (dir + Vector3.up * 0.5f) * data.knockbackForce;
        ApplyRagdoll(force);

        if (baseAI != null)
            baseAI.TakeDamage(data.damage);
    }

    public void LightningStrikeReaction(GameObject magic, MagicDataSO data, int attackerActorNr)
    {
        Vector3 dir = (transform.position - magic.transform.position).normalized;
        Vector3 force = (dir + Vector3.up * 0.5f) * data.knockbackForce;
        ApplyRagdoll(force);

        if (baseAI != null)
            baseAI.TakeDamage(data.damage);
    }

    public void TornadoReaction()
    {
        ApplyRagdoll(Vector3.zero);
    }
}
