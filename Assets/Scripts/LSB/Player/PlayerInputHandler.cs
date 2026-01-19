using Photon.Pun;
using System;
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
    private InputAction _transformAction;

    #region 이벤트 정의
    public event Action<Vector2> OnMoveEvent;   // 이동
    public event Action OnJumpEvent;            // 점프
    public event Action<bool> OnAttackEvent;    // 공격
    public event Action<bool> OnTransformEvent; // 변신
    #endregion

    #region 프로퍼티
    public Vector2 MoveInput => _areInputsAllowed ? _rawMoveInput : Vector2.zero;
    public bool IsSprintInput { get; private set; }
    public bool IsWalkInput { get; private set; }
    public bool JumpButtonHeld { get; private set; }
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
        ConnectMagicSelectInput();
        ConnetAttackInput();
        ConnetTransformationInput();

        UIManager.Instance.onOpenUI += DisableInputLogic;
        UIManager.Instance.onCloseUI += EnableInputLogic;
    }

    private void OnDisable()
    {
        if (!photonView.IsMine || _moveAction == null) return;

        ResetInputs();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.onOpenUI -= DisableInputLogic;
            UIManager.Instance.onCloseUI -= EnableInputLogic;
        }
    }

    private void ResetInputs()
    {
        _rawMoveInput = Vector2.zero;
        IsSprintInput = false;
        IsWalkInput = false;
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

        _jumpAction.performed += ctx =>
        {
            if (_areInputsAllowed)
            {
                OnJumpEvent?.Invoke();
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

    private void ConnectMagicSelectInput()
    {
        _selectQAction = _playerInput.actions["SelectQ"];
        _selectEAction = _playerInput.actions["SelectE"];
    }

    private void ConnetAttackInput()
    {
        _attackLeftAction = _playerInput.actions["AttackLeft"];
        _attackRightAction = _playerInput.actions["AttackRight"];

        _attackLeftAction.performed += ctx => HandleAttackInput(true);
        _attackRightAction.performed += ctx => HandleAttackInput(false);
    }

    private void HandleAttackInput(bool isLeft)
    {
        if (!_player.TransformationController.IsWizard || _player.TransformationController.IsTransforming) return;

        if (_areInputsAllowed && !_isQHold && !_isEHold)
        {
            OnAttackEvent?.Invoke(isLeft);
        }
    }

    private void ConnetTransformationInput()
    {
        _transformAction = _playerInput.actions["Transformation"];

        _transformAction.started += ctx =>
        {
            if (_areInputsAllowed) OnTransformEvent?.Invoke(true);
        };

        _transformAction.canceled += ctx =>
        {
            OnTransformEvent?.Invoke(false);
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
            if (_moveAction != null) 
                _rawMoveInput = _moveAction.ReadValue<Vector2>();
        }
    }
}
#region 레거시 코드
//using Photon.Pun;
//using UnityEngine;
//using UnityEngine.InputSystem;

//public class PlayerInputHandler : MonoBehaviourPun
//{
//    private PlayerInput _playerInput;
//    private PlayableCharacter _player;

//    private Vector2 _rawMoveInput;
//    private bool _areInputsAllowed = true;



//    private float _qPressTime;
//    private float _ePressTime;
//    private bool _isQHold;
//    private bool _isEHold;
//    private const float HOLD_SELECTTIME = 0.5f;

//    private InputAction _moveAction;
//    private InputAction _jumpAction;
//    private InputAction _walkAction;
//    private InputAction _sprintAction;
//    private InputAction _selectQAction;
//    private InputAction _selectEAction;
//    private InputAction _attackLeftAction;
//    private InputAction _attackRightAction;
//    private InputAction _transformAction;

//    #region 프로퍼티
//    public Vector2 MoveInput => _areInputsAllowed ? _rawMoveInput : Vector2.zero;
//    public bool IsSprintInput { get; private set; }
//    public bool IsWalkInput { get; private set; }
//    public bool JumpTriggered { get; private set; }
//    public bool JumpButtonHeld { get; private set; }
//    public bool AttackLeftTriggered { get; private set; }
//    public bool AttackRightTriggered { get; private set; }
//    #endregion



//    private void Awake()
//    {
//        _playerInput = GetComponent<PlayerInput>();
//        _player = GetComponent<PlayableCharacter>();
//    }

//    private void OnEnable()
//    {
//        if (!photonView.IsMine)
//        {
//            _playerInput.enabled = false;
//            return;
//        }

//        ConnectMove();
//        ConnectJump();
//        ConnectWalk();
//        ConnectSprint();
//        ConnectMagicSelectInput();
//        ConnetAttackInput();
//        ConnetTransformationInput();

//        UIManager.Instance.onOpenUI += DisableInputLogic;
//        UIManager.Instance.onCloseUI += EnableInputLogic;
//    }

//    private void LateUpdate()
//    {
//        if (!photonView.IsMine) return;

//        JumpTriggered = false;
//        AttackLeftTriggered = false;
//        AttackRightTriggered = false;
//    }

//    private void OnDisable()
//    {
//        if (!photonView.IsMine || _moveAction == null) return;

//        ResetInputs();

//        if (UIManager.Instance != null)
//        {
//            UIManager.Instance.onOpenUI -= DisableInputLogic;
//            UIManager.Instance.onCloseUI -= EnableInputLogic;
//        }
//    }

//    private void ResetInputs()
//    {
//        _rawMoveInput = Vector2.zero;
//        IsSprintInput = false;
//        IsWalkInput = false;
//        JumpTriggered = false;
//        JumpButtonHeld = false;
//    }


//    private void ConnectMove()
//    {
//        _moveAction = _playerInput.actions["Move"];
//        _moveAction.performed += ctx => _rawMoveInput = ctx.ReadValue<Vector2>();
//        _moveAction.canceled += ctx => _rawMoveInput = Vector2.zero;
//    }

//    private void ConnectJump()
//    {
//        _jumpAction = _playerInput.actions["Jump"];

//        _jumpAction.started += ctx =>
//        {
//            if (_areInputsAllowed)
//            {
//                JumpTriggered = true;
//                JumpButtonHeld = true;
//            }
//        };

//        _jumpAction.canceled += ctx => JumpButtonHeld = false;
//    }

//    private void ConnectWalk()
//    {
//        _walkAction = _playerInput.actions["Walk"];
//        _walkAction.started += ctx => IsWalkInput = true;
//        _walkAction.canceled += ctx => IsWalkInput = false;
//    }

//    private void ConnectSprint()
//    {
//        _sprintAction = _playerInput.actions["Sprint"];
//        _sprintAction.started += ctx => IsSprintInput = true;
//        _sprintAction.canceled += ctx => IsSprintInput = false;
//    }

//    private void ConnectMagicSelectInput()
//    {
//        _selectQAction = _playerInput.actions["SelectQ"];
//        _selectEAction = _playerInput.actions["SelectE"];
//    }

//    private void ConnetAttackInput()
//    {
//        _attackLeftAction = _playerInput.actions["AttackLeft"];
//        _attackRightAction = _playerInput.actions["AttackRight"];

//        _attackLeftAction.performed += ctx =>
//        {
//            if (!_player.TransformationController.IsWizard || _player.TransformationController.IsTransforming) return;
//            if (_areInputsAllowed && !_isQHold && !_isEHold)
//                AttackLeftTriggered = true;
//        };

//        _attackRightAction.performed += ctx =>
//        {
//            if (!_player.TransformationController.IsWizard || _player.TransformationController.IsTransforming) return;
//            if (_areInputsAllowed && !_isQHold && !_isEHold)
//                AttackRightTriggered = true;
//        };
//    }

//    private void ConnetTransformationInput()
//    {
//        _transformAction = _playerInput.actions["Transformation"];
//        _transformAction.started += ctx =>
//        {
//            if (_areInputsAllowed)
//                _player.TransformationController.HandleTransformInput(true);
//        };
//        _transformAction.canceled += ctx =>
//        {
//            if (_areInputsAllowed)
//                _player.TransformationController.HandleTransformInput(false);
//        };

//    }

//    private void EnableInputLogic() => OnPlayerInput();
//    private void DisableInputLogic() => OffPlayerInput();

//    public void OffPlayerInput()
//    {
//        if (photonView.IsMine)
//        {
//            _areInputsAllowed = false;
//            _rawMoveInput = Vector2.zero;
//        }
//    }

//    public void OnPlayerInput()
//    {
//        if (photonView.IsMine)
//        {
//            _areInputsAllowed = true;
//            if(_moveAction != null) _rawMoveInput = _moveAction.ReadValue<Vector2>();
//        }
//    }
//}
#endregion