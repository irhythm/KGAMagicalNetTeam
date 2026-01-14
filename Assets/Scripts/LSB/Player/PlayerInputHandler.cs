using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviourPun
{
    private PlayerInput _playerInput;

    private Vector2 _rawMoveInput;
    private bool _areInputsAllowed = true;

    #region Public Properties (State에서 접근)

    public Vector2 MoveInput => _areInputsAllowed ? _rawMoveInput : Vector2.zero;

    public bool IsSprintInput { get; private set; }
    public bool IsWalkInput { get; private set; }

    public bool JumpTriggered { get; private set; }

    public bool JumpButtonHeld { get; private set; }

    #endregion

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

        GameManager.Instance.onOpenUI += DisableInputLogic;
        GameManager.Instance.onCloseUI += EnableInputLogic;
    }

    private void LateUpdate()
    {
        if (!photonView.IsMine) return;

        JumpTriggered = false;
    }

    private void OnDisable()
    {
        if (!photonView.IsMine || _moveAction == null) return;

        ResetInputs();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.onOpenUI -= DisableInputLogic;
            GameManager.Instance.onCloseUI -= EnableInputLogic;
        }
    }

    private void ResetInputs()
    {
        _rawMoveInput = Vector2.zero;
        IsSprintInput = false;
        IsWalkInput = false;
        JumpTriggered = false;
        JumpButtonHeld = false;
    }


    private void ConnectMove()
    {
        _moveAction = _playerInput.actions["Move"];
        _moveAction.performed += ctx => _rawMoveInput = ctx.ReadValue<Vector2>();
        _moveAction.canceled += ctx => _rawMoveInput = Vector2.zero;
    }

    private void ConnectJump()
    {
        _jumpAction = _playerInput.actions["Jump"];

        _jumpAction.started += ctx =>
        {
            if (_areInputsAllowed)
            {
                JumpTriggered = true;
                JumpButtonHeld = true;
            }
        };

        _jumpAction.canceled += ctx => JumpButtonHeld = false;
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

    private void EnableInputLogic() => OnPlayerInput();
    private void DisableInputLogic() => OffPlayerInput();

    public void OffPlayerInput()
    {
        if (photonView.IsMine)
        {
            _areInputsAllowed = false;
            _rawMoveInput = Vector2.zero;
        }
    }

    public void OnPlayerInput()
    {
        if (photonView.IsMine)
        {
            _areInputsAllowed = true;
            if(_moveAction != null) _rawMoveInput = _moveAction.ReadValue<Vector2>();
        }
    }
}