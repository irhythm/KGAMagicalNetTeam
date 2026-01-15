using Photon.Pun;
using UnityEngine;

public class PlayableCharacter : MonoBehaviourPun
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float dodgeForce = 7f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDist = 0.1f;

    public enum MoveDir { Front, Back, Left, Right }

    #region 프로퍼티
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    public float JumpForce => jumpForce;
    public float DodgeForce => dodgeForce;
    #endregion

    #region 참조
    public PlayerInputHandler InputHandler { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public Animator Animator { get; private set; }
    public ThirdPersonCamera GameCamera { get; private set; }
    public PlayerInventory Inventory { get; private set; }
    public PlayerMagicSystem MagicSystem { get; private set; }
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
        Inventory = GetComponent<PlayerInventory>();
        MagicSystem = GetComponent<PlayerMagicSystem>();

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
        StateMachine.CurrentState.Execute();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        StateMachine.CurrentState.FixedExecute();
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