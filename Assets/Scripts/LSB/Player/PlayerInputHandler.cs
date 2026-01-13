using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviourPun
{
    private PlayerInput _playerInput;

    public Vector2 MoveInput { get; private set; }

    public bool IsSprintInput { get; private set; }
    public bool IsWalkInput { get; private set; }
    public bool IsJumpInput { get; private set; }

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _walkAction;
    private InputAction _sprintAction;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        if (!photonView.IsMine)
        {
            _playerInput.enabled = false;
            return;
        }

        ConnectMove();
        ConnectJump();
        ConnectWalk();
        ConnectSprint();

        GameManager.Instance.onOpenUI += CheckDisable;
        GameManager.Instance.onCloseUI += CheckEnable;
    }

    private void LateUpdate()
    {
        if (!photonView.IsMine) return;

        OffInputs();
    }

    private void OnDisable()
    {
        if (!photonView.IsMine || _moveAction == null) return;

        MoveInput = Vector2.zero;
        IsSprintInput = false;
        IsWalkInput = false;
        IsJumpInput = false;

        CheckEnable();

        GameManager.Instance.onOpenUI -= CheckDisable;
        GameManager.Instance.onCloseUI -= CheckEnable;
    }

    private void CheckEnable()
    {
        if (!photonView.IsMine)
            return;
        _moveAction.Enable();
        _jumpAction.Enable();
        _walkAction.Enable();
        _sprintAction.Enable();
        Debug.Log("움직임 가능");
    }
    private void CheckDisable()
    {
        if (!photonView.IsMine)
            return;
        _moveAction.Disable();
        _jumpAction.Disable();
        _walkAction.Disable();
        _sprintAction.Disable();
        Debug.Log("움직임 불가능");
    }

    private void ConnectMove()
    {
        _moveAction = _playerInput.actions["Move"];
        _moveAction.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        _moveAction.canceled += ctx => MoveInput = Vector2.zero;
    }

    private void ConnectJump()
    {
        _jumpAction = _playerInput.actions["Jump"];
        _jumpAction.started += ctx => IsJumpInput = true;
    }

    private void ConnectWalk()
    {
        _walkAction = _playerInput.actions["Walk"];

        _walkAction.started += ctx => IsWalkInput = true;
        _walkAction.canceled += ctx => IsWalkInput = false;
    }

    private void ConnectSprint()
    {
        _sprintAction = _playerInput.actions["Sprint"];

        _sprintAction.started += ctx => IsSprintInput = true;
        _sprintAction.canceled += ctx => IsSprintInput = false;
    }

    private void OffInputs()
    {
        IsJumpInput = false;
    }
}