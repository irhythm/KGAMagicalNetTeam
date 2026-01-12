using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviourPun
{
    // 커멘드 패턴이란?
    // 키설정변경, 입력 기록 저장, 실행 취소, 리플레이, 무르기 기능 등등
    // 요청을 객체로 캡슐화, 실행 주체와 요청자를 분리한다
    // 요청을 저장하고 실행/취소/재실행/이 가능하도록 설계 (커스텀 가능)

    // 고로 우리가 생각해야 할 것은 딱 3개
    // 1. Invoker : 발생자(키보드 입력, 게임패드 입력, 버튼 등등)
    // 2. Command : 그 입력에 의해서 발동되는 행동 (공격, 이동, 점프 등등)
    // 3. Receiver : 그거 보고 행동을 하는 진짜 캐릭터 코딩


    private PlayerInput _playerInput;

    public Vector2 MoveInput { get; private set; }
    public bool IsJumpTriggered { get; private set; }

    private InputAction _moveAction;
    private InputAction _jumpAction;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        // 내 캐릭터라면 액션을 찾아서 연결
        if (!photonView.IsMine)
        {
            _playerInput.enabled = false;
            return;
        }

        _moveAction = _playerInput.actions["Move"];
        _jumpAction = _playerInput.actions["Jump"];

        _moveAction.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        _moveAction.canceled += ctx => MoveInput = Vector2.zero;

        _jumpAction.started += ctx => IsJumpTriggered = true;
    }

    private void LateUpdate()
    {
        if (!photonView.IsMine) return;
        IsJumpTriggered = false;
    }

    private void OnDisable()
    {
        // 내 캐릭터가 아니면 이미 _moveAction이 null일 수 있으므로 체크
        if (!photonView.IsMine || _moveAction == null) return;

        MoveInput = Vector2.zero;
    }
}