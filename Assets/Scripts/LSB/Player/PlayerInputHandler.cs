using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviourPun
{
    private PlayerInput _playerInput;
    private PlayableCharacter _player;

    private Vector2 _rawMoveInput;
    private bool _areInputsAllowed = true;

    

    private float _qPressTime;
    private float _ePressTime;
    private bool _isQHold;
    private bool _isEHold;
    private const float HOLD_SELECTTIME = 0.5f;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _walkAction;
    private InputAction _sprintAction;
    private InputAction _selectQAction;
    private InputAction _selectEAction;
    private InputAction _attackLeftAction;
    private InputAction _attackRightAction;


    #region 프로퍼티
    public Vector2 MoveInput => _areInputsAllowed ? _rawMoveInput : Vector2.zero;
    public bool IsSprintInput { get; private set; }
    public bool IsWalkInput { get; private set; }
    public bool JumpTriggered { get; private set; }
    public bool JumpButtonHeld { get; private set; }
    public bool AttackLeftTriggered { get; private set; }
    public bool AttackRightTriggered { get; private set; }
    #endregion

    

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _player = GetComponent<PlayableCharacter>();
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
        ConnectInputActions();

        GameManager.Instance.onOpenUI += DisableInputLogic;
        GameManager.Instance.onCloseUI += EnableInputLogic;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        HandleHoldLogic();
    }

    private void LateUpdate()
    {
        if (!photonView.IsMine) return;

        JumpTriggered = false;
        AttackLeftTriggered = false;
        AttackRightTriggered = false;
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

    private void ConnectInputActions()
    {
        _selectQAction = _playerInput.actions["SelectQ"];
        _selectEAction = _playerInput.actions["SelectE"];

        _attackLeftAction = _playerInput.actions["AttackLeft"];
        _attackRightAction = _playerInput.actions["AttackRight"];

        _attackLeftAction.performed += ctx =>
        {
            if (_areInputsAllowed && !_isQHold && !_isEHold)
                AttackLeftTriggered = true;
        };

        _attackRightAction.performed += ctx =>
        {
            if (_areInputsAllowed && !_isQHold && !_isEHold)
                AttackRightTriggered = true;
        };
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













    private void HandleHoldLogic()
    {
        if (_selectQAction.IsPressed())
        {
            if (!_isQHold)
            {
                _qPressTime += Time.deltaTime;
                if (_qPressTime >= HOLD_SELECTTIME)
                {
                    _isQHold = true;
                    OpenSelector(true);
                }
            }
        }
        else
        {
            if (_isQHold)
            {
                CloseSelector();
                _isQHold = false;
            }
            _qPressTime = 0f;
        }

        if (_selectEAction.IsPressed())
        {
            if (!_isEHold)
            {
                _ePressTime += Time.deltaTime;
                if (_ePressTime >= HOLD_SELECTTIME)
                {
                    _isEHold = true;
                    OpenSelector(false);
                }
            }
        }
        else
        {
            if (_isEHold)
            {
                CloseSelector();
                _isEHold = false;
            }
            _ePressTime = 0f;
        }
    }
    private void OpenSelector(bool isLeft)
    {
        _player.MagicSelector.Open(isLeft);

        if (_player.GameCamera != null)
        {
            _player.GameCamera.SetControl(false);
        }
    }

    private void CloseSelector()
    {
        _player.MagicSelector.Close();

        if (_player.GameCamera != null)
        {
            _player.GameCamera.SetControl(true);
        }
    }
}