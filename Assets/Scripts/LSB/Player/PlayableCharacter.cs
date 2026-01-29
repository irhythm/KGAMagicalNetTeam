using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayableCharacter : MonoBehaviourPun, IInteractable
{
    // 구독할 이벤트 정의
    public event Action<float> OnHpChanged; // 체력 변경하면 전달용
    public event Action OnDie;              // 사망하면 호출용

    // 모델
    private PlayerModel _model;
    public PlayerModel Model=>_model;

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

    [Header("Transformation")]
    [SerializeField] private GameObject civilianModel;
    [SerializeField] private GameObject wizardModel;

    [Header("MapIcon")]
    [SerializeField] private MapIcon playerIcon;
    [SerializeField] private MapIcon teamIcon;

    [Header("Interact Settings")]
    [SerializeField] private LayerMask canInteractLayer;    // 상호작용이 가능한 레이어
    [SerializeField] private float checkDistance = 1f;      // 260122 신현섭: 상호작용 체크 거리

    [Header("Camera Setting")]
    [SerializeField] private int cameraIndex = -1;

    public HashSet<IInteractable> receivers = new HashSet<IInteractable>();

    public enum MoveDir { Front, Back, Left, Right }

    #region 프로퍼티
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    public float JumpForce => jumpForce;
    public float DodgeForce => dodgeForce;
    public float LastDodgeTime { get; set; } = 0f;
    public bool CanDodge => Time.time >= LastDodgeTime + DodgeCooldown;
    public GameObject CivilianModel => civilianModel;
    public GameObject WizardModel => wizardModel;
    #endregion

    #region 참조
    public PlayerInputHandler InputHandler { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public Animator Animator { get; private set; }
    public ThirdPersonCamera GameCamera { get; private set; }
    public PlayerInventory Inventory { get; private set; }
    public PlayerMagicSystem MagicSystem { get; private set; }
    public PlayerController playerController { get; private set; }
    public PlayerTransformationController TransformationController { get; private set; }
    public List<Transform> otherPlayerTransform = new List<Transform>();
    #endregion

    #region 상태 머신
    public StateMachine StateMachine { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerDodgeState DodgeState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerInteractState InteractState { get; private set; }    // 260122 신현섭: 상호작용 상태로 전환

    public bool IsInteracted { get; private set; }  // 260122 신현섭: IInteractable 인터페이스 필드 → 상호작용이 진행 중이면 true

    [SerializeField] private Transform currentTransform;

    public Transform ActorTrans => currentTransform;

    [field: SerializeField] public InteractionDataSO interactionData { get; set; }  // 260126 신현섭: 암살 연출 데이터


    #endregion

    #region 애니메이션
    public readonly int HashSpeed = Animator.StringToHash("Speed");
    public readonly int HashVerticalVelocity = Animator.StringToHash("VerticalVelocity");
    public readonly int HashAttackTrigger = Animator.StringToHash("AttackTrigger");
    public readonly int HashAttackID = Animator.StringToHash("AttackID");
    public readonly int HashJumpType = Animator.StringToHash("JumpType");
    public readonly int HashDodgeType = Animator.StringToHash("DodgeType");
    public readonly int HashTransform = Animator.StringToHash("Transform");

    public void UpdateMoveAnimation(float currentSpeed)
    {
        Animator.SetFloat(HashSpeed, currentSpeed, 0.1f, Time.deltaTime);
    }
    #endregion

    private void Awake()
    {
        Debug.Log("플레이어가 생성되었다");
        InputHandler = GetComponent<PlayerInputHandler>();
        Rigidbody = GetComponent<Rigidbody>();
        MagicSystem = GetComponent<PlayerMagicSystem>();
        playerController = GetComponent<PlayerController>();
        TransformationController = GetComponent<PlayerTransformationController>();

        // 모델 초기화
        _model = new PlayerModel(maxHp);
        _model.Init();

        Inventory = new PlayerInventory();

        StateMachine = new StateMachine();
        MoveState = new PlayerMoveState(this, StateMachine);
        JumpState = new PlayerJumpState(this, StateMachine, "IsJumping");
        DodgeState = new PlayerDodgeState(this, StateMachine, "IsDodging");
        AttackState = new PlayerAttackState(this, StateMachine);
        InteractState = new PlayerInteractState(this, StateMachine);    // 260122 신현섭: 상호작용 상태 생성



        //260119 최정욱 local player 인스턴스 저장
        if (photonView.IsMine)
        {
            GameManager.Instance.LocalPlayer = gameObject;
            PhotonNetwork.LocalPlayer.SetProps(NetworkProperties.PLAYER_ALIVE, true);
        }

        // 260121 신현섭 : 미니맵 연동 및 아이콘 지정
        if (photonView.IsMine)
        {
            // 미니맵 카메라에 플레이어 트랜스폼 할당
            MinimapCamera camera = FindAnyObjectByType<MinimapCamera>();

            if (camera != null)
            {
                camera.SetTarget(transform);
            }

            // 플레이어 자신일 때 플레이어 아이콘을 활성화
            playerIcon.gameObject.SetActive(true);
        }
        else
        {
            // 다른 팀원일 때 팀원 아이콘 활성화
            teamIcon.gameObject.SetActive(true);
        }
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

            // 인풋 이벤트 구독
            SubscribeInputEvents();

            StateMachine.InitState(MoveState);
        }
    }

    private void OnDisable()
    {
        if (photonView.IsMine)
        {
            //260128 최정욱 카메라 초기화
            GameCamera = null;
            // 메모리 누수 방지용 구독 해제
            UnsubscribeInputEvents();
            if(GameManager.Instance != null)
                OnDie -= GameManager.Instance.CheckDie;
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (Inventory != null)
        {
            Inventory.HandleCooldowns(Time.deltaTime);
        }

        CanInteractMotion();

        StateMachine.CurrentState.Execute();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        StateMachine.CurrentState.FixedExecute();
    }

    private void SubscribeInputEvents()
    {
        if (InputHandler == null) return;
        InputHandler.OnJumpEvent += HandleJump;
        InputHandler.OnAttackEvent += HandleAttack;
        InputHandler.OnTransformEvent += HandleTransformation;
        InputHandler.OnInteractMotionEvent += HandleInteract;
    }

    private void UnsubscribeInputEvents()
    {
        if (InputHandler == null) return;
        InputHandler.OnJumpEvent -= HandleJump;
        InputHandler.OnAttackEvent -= HandleAttack;
        InputHandler.OnTransformEvent -= HandleTransformation;
        InputHandler.OnInteractMotionEvent -= HandleInteract;
        InputHandler.DisconnectCameraChange();
    }

    // 점프/회피 이벤트
    private void HandleJump()
    {
        // 이동 상태일때만 점프/회피 가능
        if (StateMachine.CurrentState is PlayerMoveState)
        {
            Vector2 input = InputHandler.MoveInput;
            MoveDir dir = GetMoveDir(input);

            // 좌우 이동 중이면 회피
            if (dir == MoveDir.Left || dir == MoveDir.Right)
            {
                if (CanDodge)
                {
                    LastDodgeTime = Time.time;
                    StateMachine.ChangeState(DodgeState);
                }
            }
            else
            {
                StateMachine.ChangeState(JumpState);
            }
        }
    }

    private void HandleAttack(bool isLeftHand)
    {
        // 이동 상태일때만 공격 가능
        if (!(StateMachine.CurrentState is PlayerMoveState)) return;

        ActionBase magic = MagicSystem.GetAction(isLeftHand);

        // 마법 쿨타임중인지 확인
        if (MagicSystem.IsActionReady(isLeftHand))
        {
            // 공격 상태로 전환
            var attackState = AttackState as PlayerAttackState;
            if (attackState != null)
            {
                attackState.Init(isLeftHand, magic);
                StateMachine.ChangeState(AttackState);
            }
        }
        else
        {
            Debug.Log("쿨타임 중이라 공격 불가");
        }
    }

    private void HandleTransformation(bool isPressed)
    {
        TransformationController.HandleTransformInput(isPressed);
    }

    // 20260127 신현섭: 상호작용 이벤트에 체인시킬 메서드
    private void HandleInteract()
    {
        InteractionManager.Instance.RequestInteraction(interactionData, this, receivers.ToArray());
    }

    // 260122 신현섭: 상호작용이 가능한 상태인지 체크 (레이캐스트)
    private void CanInteractMotion()
    {
        if (!(StateMachine.CurrentState is PlayerMoveState)) return;

        if(Physics.Raycast(transform.position + new Vector3(0, 1, 0), Camera.main.transform.forward, out RaycastHit hit, checkDistance, canInteractLayer))
        {
            if(hit.collider.TryGetComponent<IInteractable>(out var interact) && transform.IsTargetInDirection(interact.ActorTrans, DirectionType.Backward, 110f))
            {
                // todo: 상호작용 가능 UI 띄우기 등 실행
                InputHandler.CanInteractMotion = true;

                // 상호작용 대상이 다를 경우 갱신
                if(InteractState.target != interact)
                {
                    InteractState.SetTarget(interact);
                    InteractState.Init(interact.interactionData);
                    receivers.Add(interact);
                }
            }
            else
            {
                InputHandler.CanInteractMotion = false;
                receivers.Clear();
            }
        }
    }

    public void OnAttacked(float damage)
    {
        if (photonView.IsMine == true && !PhotonNetwork.LocalPlayer.GetProps<bool>(NetworkProperties.PLAYER_ALIVE))
            return;
        photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, damage);
    }

    [PunRPC]
    public void RPC_TakeDamage(float damage)
    {
        bool isDie = _model.TakeDamage(damage);
        OnHpChanged?.Invoke(_model.CurHp / _model.MaxHp);

        if (isDie)
        {
            if (photonView.IsMine)
            {
                CheckCameraOnDie();
                PhotonNetwork.LocalPlayer.SetProps(NetworkProperties.PLAYER_ALIVE, false);
            }
            OnDie?.Invoke();
            Debug.Log("캐릭터 사망");
        }
    }

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

    public bool CheckIsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDist + 0.1f, groundLayer);
    }

    public void SetAnimator(Animator newAnimator)
    {
        this.Animator = newAnimator;
    }

    public void ChangePlayerLayer()
    {
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    public void RemoveLayer()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    // 260122 신현섭: 상호작용 시 진행할 것들
    public void OnInteraction()
    {
        // 인풋시스템 x
        InputHandler.OffPlayerInput();
    }

    public void OnStopped()
    {
        // 인풋시스템 o
        InputHandler.OnPlayerInput();
    }

    public void CheckCameraOnDie()
    {
        if (!photonView.IsMine)
            return;
        //1. 조작권 박탈 <- 인풋핸들러 쪽으로 이양
        
        //2. 카메라 타겟 다른 플레이어로 전환
        if (otherPlayerTransform.Count <= 0)
        {
            PlayableCharacter[] otherPlayer = FindObjectsByType<PlayableCharacter>(FindObjectsSortMode.None);

            foreach (PlayableCharacter p in otherPlayer)
            {
                if (!p.GetComponent<PhotonView>().IsMine)
                    otherPlayerTransform.Add(p.transform);
            }
        }
        //3. 특정 버튼 클릭 시 다른 플레이어 확인 가능 여기서 액션 버튼 +=으로 넣고 파괴시 빼자
        if (otherPlayerTransform.Count > 0)
        {
            ChangeCameraTarget();
            InputHandler.ConnectCameraChange();
        }
    }

    public void ChangeCameraTarget()
    {
        cameraIndex++;
        if (cameraIndex >= otherPlayerTransform.Count)
            cameraIndex = 0;

        int checkCount = 0;

        while (true)
        {      
            Transform t = otherPlayerTransform[cameraIndex];
            if (t != null)
            {
                PhotonView pv = t.GetComponent<PhotonView>();
                if (pv.Owner.GetProps<bool>(NetworkProperties.PLAYER_ALIVE))
                {
                    break;
                }
            }
            cameraIndex++;
            if (cameraIndex >= otherPlayerTransform.Count)
                cameraIndex = 0;
            checkCount++;
            if (checkCount >= otherPlayerTransform.Count)
            {
                Debug.Log("생존자 없음");
                break;
            }
        }
        if (otherPlayerTransform[cameraIndex] != null) 
        {
            GameCamera.SetTarget(otherPlayerTransform[cameraIndex]);
        }
    }
    public void ChangeCameraTargetOnPlayerInput(InputAction.CallbackContext ctx)
    {
        ChangeCameraTarget();
    }
}



#region 레거시 코드
//using Photon.Pun;
//using System;
//using UnityEngine;

//public class PlayableCharacter : MonoBehaviourPun
//{
//    // 구독할 이벤트 정의
//    public event Action<float> OnHpChanged; // 체력 변경하면 전달용
//    public event Action OnDie;              // 사망하면 호출용

//    // 모델
//    private PlayerModel _model;

//    [Header("Settings")]
//    [SerializeField] private float maxHp = 100f; // 체력 인스펙터 노출
//    [SerializeField] private float moveSpeed = 5f;
//    [SerializeField] private float rotationSpeed = 10f;
//    [SerializeField] private float jumpForce = 5f;
//    [SerializeField] private float dodgeForce = 7f;
//    [SerializeField] private float DodgeCooldown = 1.5f;


//    [Header("Ground Detection")]
//    [SerializeField] private LayerMask groundLayer;
//    [SerializeField] private float groundCheckDist = 0.1f;

//    public enum MoveDir { Front, Back, Left, Right }

//    #region 프로퍼티
//    public float MoveSpeed => moveSpeed;
//    public float RotationSpeed => rotationSpeed;
//    public float JumpForce => jumpForce;
//    public float DodgeForce => dodgeForce;
//    public float LastDodgeTime { get; set; } = 0f;
//    public bool CanDodge => Time.time >= LastDodgeTime + DodgeCooldown;
//    #endregion

//    #region 참조
//    public PlayerInputHandler InputHandler { get; private set; }
//    public Rigidbody Rigidbody { get; private set; }
//    public Animator Animator { get; private set; }
//    public ThirdPersonCamera GameCamera { get; private set; }
//    public PlayerInventory Inventory { get; private set; }
//    public PlayerMagicSystem MagicSystem { get; private set; }
//    public PlayerController playerController { get; private set; }
//    public PlayerTransformationController TransformationController { get; private set; }
//    #endregion

//    #region 상태 머신
//    public StateMachine StateMachine { get; private set; }
//    public PlayerMoveState MoveState { get; private set; }
//    public PlayerJumpState JumpState { get; private set; }
//    public PlayerDodgeState DodgeState { get; private set; }
//    public PlayerAttackState AttackState { get; private set; }
//    #endregion



//    private void Awake()
//    {
//        InputHandler = GetComponent<PlayerInputHandler>();
//        Rigidbody = GetComponent<Rigidbody>();
//        MagicSystem = GetComponent<PlayerMagicSystem>();
//        playerController = GetComponent<PlayerController>();
//        TransformationController = GetComponent<PlayerTransformationController>();

//        // 모델 초기화
//        _model = new PlayerModel(maxHp);
//        _model.Init();

//        Inventory = new PlayerInventory();

//        StateMachine = new StateMachine();
//        MoveState = new PlayerMoveState(this, StateMachine);
//        JumpState = new PlayerJumpState(this, StateMachine, "IsJumping");
//        DodgeState = new PlayerDodgeState(this, StateMachine, "IsDodging");
//        AttackState = new PlayerAttackState(this, StateMachine, "IsAttacking");
//    }

//    private void Start()
//    {
//        if (photonView.IsMine)
//        {
//            var camScript = FindAnyObjectByType<ThirdPersonCamera>();
//            if (camScript != null)
//            {
//                GameCamera = camScript;
//                camScript.SetTarget(this.transform);
//            }

//            StateMachine.InitState(MoveState);
//        }
//    }

//    private void Update()
//    {
//        if (!photonView.IsMine) return;

//        if (Inventory != null)
//        {
//            Inventory.HandleCooldowns(Time.deltaTime);
//        }

//        StateMachine.CurrentState.Execute();
//    }

//    private void FixedUpdate()
//    {
//        if (!photonView.IsMine) return;
//        StateMachine.CurrentState.FixedExecute();
//    }


//    // 데미지 처리 로직 여기로 옮김
//    public void OnAttacked(float damage)
//    {
//        photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, damage);
//    }

//    // 데미지 적용 이벤트 호출
//    [PunRPC]
//    public void RPC_TakeDamage(float damage)
//    {
//        bool isDie = _model.TakeDamage(damage);

//        // 데이터 변경 사실을 Presenter에게 알림
//        OnHpChanged?.Invoke(_model.CurHp / _model.MaxHp);

//        if (isDie)
//        {
//            OnDie?.Invoke();
//            Debug.Log("캐릭터 사망 (Logic)");
//        }
//    }


//    /// <summary>
//    /// 움직이는 방향 구하는용도
//    /// </summary>
//    /// <param name="input"></param>
//    /// <returns></returns>
//    public MoveDir GetMoveDir(Vector2 input)
//    {
//        if (input.magnitude < 0.1f) return MoveDir.Front;

//        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
//        {
//            return input.x > 0 ? MoveDir.Right : MoveDir.Left;
//        }
//        else
//        {
//            return input.y > 0 ? MoveDir.Front : MoveDir.Back;
//        }
//    }

//    /// <summary>
//    /// 그라운드 체크
//    /// </summary>
//    /// <returns>땅인지</returns>
//    public bool CheckIsGrounded()
//    {
//        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDist + 0.1f, groundLayer);
//    }

//    public void SetAnimator(Animator newAnimator)
//    {
//        this.Animator = newAnimator;
//    }

//    public void ChangePlayerLayer()
//    {
//        gameObject.layer = LayerMask.NameToLayer("Player");
//    }

//    public void RemoveLayer()
//    {
//        gameObject.layer = LayerMask.NameToLayer("Default");
//    }
//}
#endregion