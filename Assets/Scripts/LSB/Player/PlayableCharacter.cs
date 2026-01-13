using Photon.Pun;
using UnityEngine;

public class PlayableCharacter : MonoBehaviourPun
{
    [Header("Settings")]
    public float MoveSpeed = 5f;
    public float RotationSpeed = 10f;
    public float AnimDampTime = 0.15f;

    [Header("References")]
    public PlayerInputHandler InputHandler { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public Animator Animator { get; private set; }

    public ThirdPersonCamera GameCamera { get; private set; }

    #region 상태 머신
    public StateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    #endregion

    public readonly int HashInputX = Animator.StringToHash("InputX");
    public readonly int HashInputZ = Animator.StringToHash("InputZ");

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
        if (photonView.IsMine)
        {
            // 씬에 있는 메인/TPS 카메라 찾아서 연결
            var camScript = FindAnyObjectByType<ThirdPersonCamera>();
            if (camScript != null)
            {
                GameCamera = camScript;
                camScript.SetTarget(this.transform);
            }

            StateMachine.InitState(IdleState);
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        StateMachine.CurrentState.Execute();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        StateMachine.CurrentState.FixedExecute();
    }
}