using Photon.Pun;
using System;
using UnityEngine;

public class PlayableCharacter : MonoBehaviourPun
{
    // 구독할 이벤트 정의
    public event Action<float> OnHpChanged; // 체력 변경하면 전달용
    public event Action OnDie;              // 사망하면 호출용

    // 모델
    private PlayerModel _model;

    [Header("Settings")]
    [SerializeField] private float maxHp = 100f; // 체력 인스펙터 노출
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float dodgeForce = 7f;
    [SerializeField] private float DodgeCooldown = 1.5f;


    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDist = 0.1f;

    public enum MoveDir { Front, Back, Left, Right }

    #region 프로퍼티
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    public float JumpForce => jumpForce;
    public float DodgeForce => dodgeForce;
    public float LastDodgeTime { get; set; } = 0f;
    public bool CanDodge => Time.time >= LastDodgeTime + DodgeCooldown;
    #endregion

    #region 참조
    public PlayerInputHandler InputHandler { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public Animator Animator { get; private set; }
    public ThirdPersonCamera GameCamera { get; private set; }
    public PlayerInventory Inventory { get; private set; }
    public PlayerMagicSystem MagicSystem { get; private set; }
    public PlayerController playerController { get; private set; }
    #endregion

    #region 상태 머신
    public StateMachine StateMachine { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerDodgeState DodgeState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    #endregion



    private void Awake()
    {
        InputHandler = GetComponent<PlayerInputHandler>();
        Rigidbody = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();
        MagicSystem = GetComponent<PlayerMagicSystem>();
        playerController = GetComponent<PlayerController>();

        // 모델 초기화
        _model = new PlayerModel(maxHp);
        _model.Init();

        Inventory = new PlayerInventory();

        StateMachine = new StateMachine();
        MoveState = new PlayerMoveState(this, StateMachine);
        JumpState = new PlayerJumpState(this, StateMachine, "IsJumping");
        DodgeState = new PlayerDodgeState(this, StateMachine, "IsDodging");
        AttackState = new PlayerAttackState(this, StateMachine, "IsAttacking");
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            var camScript = FindAnyObjectByType<ThirdPersonCamera>();
            if (camScript != null)
            {
                GameCamera = camScript;
                camScript.SetTarget(this.transform);
            }

            StateMachine.InitState(MoveState);
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (Inventory != null)
        {
            Inventory.HandleCooldowns(Time.deltaTime);
        }

        StateMachine.CurrentState.Execute();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        StateMachine.CurrentState.FixedExecute();
    }


    // 데미지 처리 로직 여기로 옮김
    public void OnAttacked(float damage)
    {
        photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, damage);
    }

    // 데미지 적용 이벤트 호출
    [PunRPC]
    public void RPC_TakeDamage(float damage)
    {
        bool isDie = _model.TakeDamage(damage);

        // 데이터 변경 사실을 Presenter에게 알림
        OnHpChanged?.Invoke(_model.CurHp / _model.MaxHp);

        if (isDie)
        {
            OnDie?.Invoke();
            Debug.Log("캐릭터 사망 (Logic)");
        }
    }


    /// <summary>
    /// 움직이는 방향 구하는용도
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public MoveDir GetMoveDir(Vector2 input)
    {
        if (input.magnitude < 0.1f) return MoveDir.Front;

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            return input.x > 0 ? MoveDir.Right : MoveDir.Left;
        }
        else
        {
            return input.y > 0 ? MoveDir.Front : MoveDir.Back;
        }
    }

    /// <summary>
    /// 그라운드 체크
    /// </summary>
    /// <returns>땅인지</returns>
    public bool CheckIsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDist + 0.1f, groundLayer);
    }
}