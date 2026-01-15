using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

/* 
모든 AI의 부모가 되는 추상 클래스
경비/시민이 가져야 할 공통 로직 공유

마스터클라이언트가 소유권을 가지고
모든 AI의 판단과 이동 연산은
마스터클라이언트 한 명만 수행, 나머지 클라이언트는 그 위치값만 받아 동기화
*/

public abstract class BaseAI : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Base Settings")]
    public float moveSpeed = 3.5f;
    public float rotSpeed = 120f;
    public float maxHP = 100f;

    public NavMeshAgent Agent { get; private set; }
    public Animator Anim { get; private set; }
    public float CurrentHP { get; protected set; }


    //현재 상태
    protected StateMachine stateMachine;

    //네트워크 동기화용 상태 인덱스, 애니에서도 쓸 듯?
    //전송은 정수로만
    public enum AIStateID { Patrol, Alert, Action, Dead, Chase, Attack }
    public AIStateID currentNetworkState;

    protected virtual void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponent<Animator>();

        Agent.speed = moveSpeed;
        Agent.angularSpeed = rotSpeed;
        CurrentHP = maxHP;

        stateMachine = new StateMachine(); //상태머신 생성
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
        }
        else
        {
            Debug.LogError("NavMesh가 없읆..");
        }
        SetInitialState();
    }


    protected abstract void SetInitialState(); //자식이 구현

    protected virtual void Update()
    {
        //마스터 클라이언트만 상태머신 돌리기
        if (PhotonNetwork.IsMasterClient)
        {
            stateMachine.CurrentState?.Execute();
        }
    }

    protected virtual void FixedUpdate()
    {
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
}
