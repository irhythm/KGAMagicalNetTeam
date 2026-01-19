using UnityEngine;

public class PlayerJumpState : PlayerStateBase
{
    private float _initJumpTime;

    private float airControlFactor = 0.7f;

    public PlayerJumpState(PlayableCharacter player, StateMachine stateMachine, string animationNum)
        : base(player, stateMachine, animationNum) { }

    public override void Enter()
    {
        base.Enter();

        player.InputHandler.OnPlayerInput();

        Vector2 input = player.InputHandler.MoveInput;

        var dir = player.GetMoveDir(input);
        player.Animator.SetInteger(player.HashJumpType, (int)dir);

        player.Rigidbody.linearVelocity = new Vector3(player.Rigidbody.linearVelocity.x, 0, player.Rigidbody.linearVelocity.z);
        player.Rigidbody.AddForce(Vector3.up * player.JumpForce, ForceMode.Impulse);

        _initJumpTime = Time.time + 0.1f;
    }

    public override void FixedExecute()
    {
        base.FixedExecute();

        HandleAirMovement();

        if (player.Rigidbody.linearVelocity.y > 0 && !player.InputHandler.JumpButtonHeld)
        {
            Vector3 vel = player.Rigidbody.linearVelocity;
            vel.y *= 0.8f;
            player.Rigidbody.linearVelocity = vel;
        }

        if (Time.time > _initJumpTime && player.CheckIsGrounded() && player.Rigidbody.linearVelocity.y <= 0.1f)
        {
            player.Rigidbody.linearVelocity = Vector3.zero;
            stateMachine.ChangeState(player.MoveState);
        }

        player.Animator.SetFloat(player.HashVerticalVelocity, player.Rigidbody.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
    }

    private void HandleAirMovement()
    {
        Vector2 input = player.InputHandler.MoveInput;

        Vector3 camForward = Vector3.forward;
        Vector3 camRight = Vector3.right;

        if (Camera.main != null)
        {
            camForward = Camera.main.transform.forward;
            camRight = Camera.main.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();
        }

        Vector3 targetDir = (camForward * input.y + camRight * input.x).normalized;

        if (targetDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            player.transform.rotation = Quaternion.Slerp(
                player.transform.rotation,
                targetRotation,
                player.RotationSpeed * Time.fixedDeltaTime
            );
        }

        Vector3 currentVelocity = player.Rigidbody.linearVelocity;

        Vector3 targetVelocity = targetDir * player.MoveSpeed * airControlFactor;

        Vector3 newHorizontalVelocity = Vector3.Lerp(
            new Vector3(currentVelocity.x, 0, currentVelocity.z),
            targetVelocity,
            Time.fixedDeltaTime * 5f
        );

        player.Rigidbody.linearVelocity = new Vector3(
            newHorizontalVelocity.x,
            currentVelocity.y,
            newHorizontalVelocity.z
        );
    }
}