using UnityEngine;

public class PlayerAttackState : PlayerStateBase
{
    public PlayerAttackState(PlayableCharacter player, StateMachine stateMachine)
        : base(player, stateMachine) { }

    private const float AnimDuration = 0.8f;

    private bool _isLeftHand;
    private ActionBase _currentAction;
    private float _finishTime;

    public void Init(bool isLeftHand, ActionBase action)
    {
        this._isLeftHand = isLeftHand;
        this._currentAction = action;
    }

    public override void Enter()
    {
        base.Enter();

        if (_currentAction == null || _currentAction.BaseData == null)
        {
            stateMachine.ChangeState(player.MoveState);
            return;
        }

        if (_isLeftHand)
            player.Animator.SetInteger(player.HashAttackID, 0);
        else
            player.Animator.SetInteger(player.HashAttackID, 1);

        player.Animator.SetTrigger(player.HashAttackTrigger);

        player.MagicSystem.UseAction(_isLeftHand);

        _finishTime = Time.time + AnimDuration;
    }

    public override void Execute()
    {
        base.Execute();

        if (Camera.main != null)
        {
            Vector3 aimDir = Camera.main.transform.forward;
            aimDir.y = 0;
            if (aimDir != Vector3.zero)
            {
                player.transform.rotation = Quaternion.LookRotation(aimDir);
            }
        }

        Vector2 input = player.InputHandler.MoveInput;
        Vector3 moveDir = Vector3.zero;

        if (input.sqrMagnitude > 0.01f && Camera.main != null)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            moveDir = (camForward * input.y + camRight * input.x).normalized;
        }

        float attackMoveSpeed = player.MoveSpeed * 0.5f;

        player.Rigidbody.linearVelocity = new Vector3(
            moveDir.x * attackMoveSpeed,
            player.Rigidbody.linearVelocity.y,
            moveDir.z * attackMoveSpeed
        );
        if (input.sqrMagnitude > 0.01f)
            player.UpdateMoveAnimation(0.5f); // Walk 모션
        else
            player.UpdateMoveAnimation(0f);   // Idle 모션

        if (Time.time >= _finishTime)
        {
            stateMachine.ChangeState(player.MoveState);
        }
    }
}