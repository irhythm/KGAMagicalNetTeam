using Photon.Pun;
using UnityEngine;

public class PlayableCharacter : MonoBehaviourPun
{
    public PlayerInputHandler InputHandler { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public Animator Animator { get; private set; }

    // State Machine 관련 변수들
    public StateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }

    public float MoveSpeed = 5f;
    public float RotationSpeed = 10f;

    private void Awake()
    {
        InputHandler = GetComponent<PlayerInputHandler>();
        Rigidbody = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();

        StateMachine = new StateMachine();
        IdleState = new PlayerIdleState(this, StateMachine);
        MoveState = new PlayerMoveState(this, StateMachine);
    }

    private void Start()
    {
        // [중요] 내 캐릭터인 경우에만 상태 머신 시작!
        // 남의 캐릭터는 상태 머신이 아니라 PhotonTransformView가 움직여줌
        if (photonView.IsMine)
        {
            StateMachine.InitState(IdleState);

            // 카메라 세팅 등이 필요하다면 여기서 호출 (예: Camera.main.Follow = this.transform)
        }
    }

    private void Update()
    {
        // [핵심] 내 캐릭터가 아니면 로직 중단
        if (!photonView.IsMine) return;

        StateMachine.CurrentState.Execute();
    }

    private void FixedUpdate()
    {
        // [핵심] 내 캐릭터가 아니면 물리 연산 중단
        if (!photonView.IsMine) return;

        StateMachine.CurrentState.FixedExecute();
    }
}
