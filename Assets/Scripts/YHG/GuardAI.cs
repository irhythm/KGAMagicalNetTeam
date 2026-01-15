using Photon.Pun;
using UnityEngine;
using System.Collections;

public class GuardAI : BaseAI
{
    [Header("경비 세팅")]
    public float detectRadius = 15f;    // 15m 감지
    public float attackRange = 1.5f;    // 공격 사거리
    public float attackCooldown = 2.0f; // 공격 쿨타임


    //속도 설정 
    public float patrolSpeed = 3.5f;
    public float runSpeed = 6.0f;
    public int damage = 1;

    [Header("플레이어 레이어")]
    public LayerMask targetMask;

    //상태용
    public Transform targetPlayer;
    public float lastAttackTime; //쿨타임 계산용

    //최적화용 버퍼
    private Collider[] connectionBuffer = new Collider[1];

    //랙돌용 컴포넌트 캐싱
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;

    protected override void Awake()
    {
        base.Awake();

        maxHP = 1f;
        CurrentHP = maxHP;
        InitRagdoll();
    }

    protected override void SetInitialState()
    {
        //ChangeState(new GuardPatrolState(this, stateMachine));
    }

    public bool CheckEnemyNearby()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectRadius, connectionBuffer, targetMask);
        if (count > 0)
        {
            targetPlayer = connectionBuffer[0].transform;
            return true;
        }
        targetPlayer = null;
        return false;
    }

    //외부에서 RPC로 호출될 피격 함수, RPC 쏘는 게 맞는 지 고민.
    [PunRPC]
    public void TakeDamage(int damageAmount)
    {
        if (currentNetworkState == AIStateID.Dead) return;

        CurrentHP -= damageAmount;

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        ChangeNetworkState(AIStateID.Dead);

        if (Agent != null)
        {
            Agent.isStopped = true;
            Agent.enabled = false;
        }

        //콜라이더 꺼두기(내일여쭤봐)
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        if (Anim != null) Anim.enabled = false;
        EnableRagdoll();

        this.enabled = false;

        //마스터클라만
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(CoDestroy());
        }
    }
    //추후 오브젝트풀링?
    IEnumerator CoDestroy()
    {
        yield return CoroutineManager.waitForSeconds(20f);
        PhotonNetwork.Destroy(gameObject);
    }

    //랙돌 초기 설정
    private void InitRagdoll()
    {
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            //본체는 제외
            if (rb.gameObject != this.gameObject)
            {
                rb.isKinematic = true; //애니메이션을 따라가도록 고정
                rb.useGravity = false;
            }
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col.gameObject != this.gameObject)
            {
                col.enabled = false; //평소에는 팔다리 콜라이더 끄기
            }
        }
    }
    //사망 시 랙돌 활성화
    private void EnableRagdoll()
    {
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb.gameObject != this.gameObject)
            {
                rb.isKinematic = false; //물리 연산 시작
                rb.useGravity = true;
            }
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col.gameObject != this.gameObject)
            {
                col.enabled = true; //충돌체 켜기
            }
        }
    }
    public void AttackTarget()
    {
        if (targetPlayer == null) return;
        //추후 타격 로직 추가
    }

    //범위체크용
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }

}
