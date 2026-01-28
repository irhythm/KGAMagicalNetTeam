using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections;

/* 
모든 AI의 부모가 되는 추상 클래스
경비/시민이 가져야 할 공통 로직 공유

01.16 랙돌/피격/사망 공유

마스터클라이언트가 소유권을 가지고
모든 AI의 판단과 이동 연산은
마스터클라이언트 한 명만 수행, 나머지 클라이언트는 그 위치값만 받아 동기화
*/

public abstract class BaseAI : MonoBehaviourPunCallbacks, IPunObservable, IDamageable, IDebuffable
{
    [Header("Base Settings")]
    public float moveSpeed = 3.5f;
    public float rotSpeed = 120f;
    public float maxHP = 100f;

    [Header("Visuals")]
    public GameObject humanModel;

    public NavMeshAgent Agent { get; private set; }
    public Animator Anim { get; private set; }
    public float CurrentHP { get; protected set; }


    //현재 상태
    protected StateMachine stateMachine;

    //네트워크 동기화용 상태 인덱스, 애니에서도 쓸 듯?
    //전송은 정수로만
    public enum AIStateID { Patrol, Alert, Action, Dead, Chase, Attack, Debuff }
    public AIStateID currentNetworkState;

    //랙돌용 컴포넌트 캐싱 01.16 이관
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;

    //01.28 맵 탈출 복귀 체크
    protected Vector3 initialPosition;

    //01.19 래그돌 체킹
    private bool _isKnockedDown = false;
    public bool IsKnockedDown
    {
        get => _isKnockedDown;
        set
        {
            _isKnockedDown = value;
            if (_isKnockedDown)
            {
                //눕는 순간 모든 AI 끄기
                if (Agent != null && Agent.isActiveAndEnabled)
                {
                    Agent.isStopped = true; //이동 정지
                    Agent.ResetPath();      //목적지 삭제
                    Agent.enabled = false;  //컴포넌트 끄기
                }
            }
        }
    }

    protected virtual void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponent<Animator>();

        Agent.speed = moveSpeed;
        Agent.angularSpeed = rotSpeed;
        CurrentHP = maxHP;

        stateMachine = new StateMachine(); //상태머신 생성

        InitRagdoll(); //랙돌 통합
    }

    protected virtual void Start()
    {
        //마스터클라이언트만 네비게이션 켜기
        if (!PhotonNetwork.IsMasterClient)
        {
            Agent.enabled = false;
            return;
        }

        Agent.enabled = true; //테스트용 이따가 삭제

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            //스폰 시점에 강제 바닥 워프
            Agent.Warp(hit.position);
            initialPosition = hit.position;
        }
        else
        {
            Debug.LogError("NavMesh가 없읆..");
            initialPosition = transform.position;
        }
        SetInitialState();
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(CoCheckMapBoundary());
        }
    }

    //맵 이탈 복귀 코루틴
    protected IEnumerator CoCheckMapBoundary()
    {
        //10초 캐싱
        var wait = CoroutineManager.waitForSeconds(10f);
        var recheckWait = CoroutineManager.waitForSeconds(3f);
        while (true)
        {
            yield return wait;
            bool isOutBound = false;

            //사망, 랙돌 상태면 패스
            if (currentNetworkState == AIStateID.Dead || IsKnockedDown) continue;
            //에이전트가 꺼져있으면 패스
            if (Agent == null || !Agent.enabled) continue;
            //공격중(나가있을리가 없음)
            if (currentNetworkState == AIStateID.Attack) continue;


            if (!Agent.isOnNavMesh)
            {
                isOutBound = true;
            }
            else if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                isOutBound = true;
            }

            //초기 위치로 복귀
            if (isOutBound)
            {
                yield return recheckWait;
                if (currentNetworkState == AIStateID.Dead || IsKnockedDown) continue;
                if (Agent == null || !Agent.enabled) continue;

                bool isConfirmedOut = !Agent.isOnNavMesh;
                if (!isConfirmedOut && !NavMesh.SamplePosition(transform.position, out NavMeshHit hitFinal, 1.0f, NavMesh.AllAreas))
                {
                    isConfirmedOut = true;
                }

                if (isConfirmedOut)
                {
                    Debug.LogWarning($"{name}가 맵 밖으로 이탈하여 초기 위치로 소환");
                    //물리력 초기화
                    Agent.velocity = Vector3.zero;
                    Agent.isStopped = true;

                    //강제 이동
                    Agent.Warp(initialPosition);

                    Agent.isStopped = false;

                    //초기상태
                    SetInitialState();
                }

            }
        }
    }

    protected abstract void SetInitialState(); //자식이 구현

    protected virtual void Update()
    {
        //래그돌 상태면 AI 회로 끊어버리기
        if (IsKnockedDown) return;

        //마스터 클라이언트만 상태머신 돌리기
        if (PhotonNetwork.IsMasterClient)
        {
            stateMachine.CurrentState?.Execute();
        }
    }

    protected virtual void FixedUpdate()
    {
        //물리 업데이트도 멈춤
        if (IsKnockedDown) return;

        //마찬가지
        if (PhotonNetwork.IsMasterClient)
        {
            stateMachine.CurrentState?.FixedExecute();
        }
    }

    //자식클래스 호출용
    public void ChangeState(IState newState)
    {
        stateMachine.ChangeState(newState);
    }

    //네트워크/애니 클립용 -> AIStateBase.Enter 시점
    public void ChangeNetworkState(AIStateID newState)
    {
        currentNetworkState = newState;
        UpdateAnimationState(); //내 화면(마스터) 애니메이션 갱신
    }

    //26.01.15 virtual 추가
    protected virtual void UpdateAnimationState()
    {
        //SetInteger 상태변경
        if(Anim) Anim.SetInteger("State", (int)currentNetworkState);
    }

    //마스터가 아닌 클라이언트가 상태를 받아오는데 쓸 메서드
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //마스터 클라이언트는 상태 전송
            stream.SendNext((int)currentNetworkState);
        }
        else
        {
            //나머지는 수신받으면 상태 적용
            int recv = (int)stream.ReceiveNext();
            AIStateID recvState = (AIStateID)recv;

            if (currentNetworkState != recvState)
            {
                currentNetworkState = recvState;
                UpdateAnimationState(); //상태에 맞게 애니메이션 갱신
            }
        }
    }

    public void TakeDamage(float damage)
    {
        photonView.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    public void RPC_TakeDamage(float damageAmount)
    {
        if (currentNetworkState == AIStateID.Dead || CurrentHP <= 0) return;
        ProcessDamage(damageAmount);
    }

    protected virtual void ProcessDamage(float damage)
    {
        CurrentHP -= damage;

        if (CurrentHP <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        ChangeNetworkState(AIStateID.Dead);

        if (Agent != null && Agent.isActiveAndEnabled && Agent.isOnNavMesh)
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }

        if (Agent != null)
        {
            Agent.enabled = false;
        }

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
                col.isTrigger = false;
            }
        }
    }

    //랙돌 피격 끝나고 일어날 때 수행할 메서드
    public virtual void OnRecoverFromKnockdown()
    {
        IsKnockedDown = false; // 기본 동작: 상태 플래그 끄기
    }

    public void ApplyDebuff(DebuffInfo info)
    {
        if (currentNetworkState == AIStateID.Dead) return;
        stateMachine.ChangeState(new DebuffState(this, stateMachine, info));
    }

    public void RemoveDebuff(DebuffType type)
    {
        if (stateMachine.CurrentState is DebuffState debuffState)
        {
            if (debuffState.CurrentInfo.Type == type)
            {
                SetInitialState();
            }
        }
    }
}
