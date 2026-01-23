using Photon.Pun;
using UnityEngine;
using System.Collections;

public class GuardAI : BaseAI
{
    [Header("경비 세팅")]
    public float detectRadius = 15f;    //15m 감지
    public float attackRange = 1.5f;    //공격 사거리
    public float attackCooldown = 2.0f; //공격 쿨타임

    [Header("공격 오조준 보정 (창병 50)")]
    public float attackRotOffset = 0f;

    [Header("루트 모션 사용 여부 (도끼병 체크)")]
    public bool useRootMotion = false;

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
    protected Collider[] connectionBuffer = new Collider[1];

    //최적화용 포톤뷰 캐싱
    protected PhotonView targetPv;

    [Header("정밀 판정용 무기 연결")]
    public MeleeWeapon[] myWeapons;


    protected override void Awake()
    {
        base.Awake();
        CurrentHP = maxHP;

        //무기에 데미지 주입
        if (myWeapons != null)
        {
            foreach (var weapon in myWeapons)
            {
                if (weapon != null) weapon.SetDamage(damage);
            }
        }
    }

    protected override void SetInitialState()
    {
        ChangeState(new GuardPatrolState(this, stateMachine));
    }

    protected override void UpdateAnimationState()
    {
        //부모 기능 실행
        base.UpdateAnimationState();

        if (Anim == null) return;

        //상태에 맞춰 Speed 주입
        switch (currentNetworkState)
        {
            case AIStateID.Patrol:
                Anim.SetFloat("Speed", 0.5f);
                break;
            case AIStateID.Chase:
                Anim.SetFloat("Speed", 1.0f);
                break;
            case AIStateID.Attack:
                Anim.SetFloat("Speed", 0f);
                break;
            case AIStateID.Dead: //랙돌이처리
                break;
            default:
                Anim.SetFloat("Speed", 0f);
                break;
        }
    }

    //01.22 virtual 추가, 주둔지 경비는 시야각+높이체크 로직 쓸 거
    public virtual bool CheckEnemyNearby()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectRadius, connectionBuffer, targetMask);
        if (count > 0)
        {
            Transform foundTarget = connectionBuffer[0].transform;
            //최적화_타겟 바뀔때만 겟컴포넌트
            if (targetPlayer != foundTarget)
            {
                targetPlayer = connectionBuffer[0].transform;
                targetPv = targetPlayer.GetComponent<PhotonView>();
            }
            return true;
        }
        targetPlayer = null;
        targetPv = null;
        return false;
    }

    //신고 기능 대리자
    //State가 GuardManager를 직접 부르지 않고 이 함수를 통해 부르도록 구조 변경
    //주둔지 경비병은 주변 동료에게만 전파로 바꿀 예정
    public virtual void ReportEnemy(Vector3 pos)
    {
        if (targetPlayer == null) return;

        PhotonView targetView = targetPlayer.GetComponent<PhotonView>();
        if (targetView != null && GuardManager.instance != null)
        {
            GuardManager.instance.ReportEnemy(targetView.Owner.ActorNumber, pos);
        }
    }

    //소음 감지 껍데기
    //플레이어가 OverlapSphere로 이 함수를 호출
    public virtual void OnHearNoise(Vector3 noisePos)
    {
        //당장은 주둔지용이고 기존 경비엔 필요한 로직이 따로 없지만 확장성 면에서 두기
    }

    public void AttackTarget()
    {
        if (targetPlayer == null) return;
        photonView.RPC("RpcPlayAttackAnim", RpcTarget.All);
        //추후 타격 로직 추가
    }

    public void InflictDamage()
    {
        //방장만 로직 실행
        if (!PhotonNetwork.IsMasterClient) return;
        if (targetPlayer == null || targetPv == null) return;

        float distance = Vector3.Distance(transform.position, targetPlayer.position);
        float hitCheckRange = attackRange + 1.0f;

        //사거리 체크
        if (distance <= hitCheckRange)
        {
            targetPv.RPC("RPC_TakeDamage", RpcTarget.All, (float)damage);
        }
    }
    //모든 클라이언트에서 공격 애니메이션 실행
    [PunRPC]
    public void RpcPlayAttackAnim()
    {
        if (Anim != null)
        {
            Anim.SetTrigger("Attack");
        }
    }

    //클립용
    public void OnHit()
    {
        if (myWeapons == null) return;
        foreach (var weapon in myWeapons)
        {
            if (weapon != null) weapon.EnableHitbox();
        }
    }

    //클립용2
    public void OnHitEnd()
    {
        if (myWeapons == null) return;
        foreach (var weapon in myWeapons)
        {
            if (weapon != null) weapon.DisableHitbox();
        }
    }

    //일어날 시 상태 초기화
    public override void OnRecoverFromKnockdown()
    {
        if (CurrentHP <= 0 || currentNetworkState == AIStateID.Dead)
        {
            return;
        }

        base.OnRecoverFromKnockdown(); // IsKnockedDown = false 실행

        //싸우던 중이면 Chase, 아니면 패트롤
        switch (currentNetworkState)
        {
            case AIStateID.Chase:
            case AIStateID.Attack:
                ChangeState(new GuardChaseState(this, stateMachine));
                break;

            case AIStateID.Patrol:
                ChangeState(new GuardPatrolState(this, stateMachine));
                break;

            default:
                ChangeState(new GuardPatrolState(this, stateMachine));
                break;
        }
    }


    //범위체크용
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }

}
