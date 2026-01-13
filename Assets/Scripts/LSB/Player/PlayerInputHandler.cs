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