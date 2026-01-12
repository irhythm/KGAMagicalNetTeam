using UnityEngine;

public class PlayerMoveState : PlayerStateBase
{
    public PlayerMoveState(PlayableCharacter player, StateMachine stateMachine) : base(player, stateMachine, "IsMove") { }

    public override void FixedExecute()
    {
        base.FixedExecute();

        Vector2 input = player.InputHandler.MoveInput;
        Vector3 moveDir = new Vector3(input.x, 0, input.y);

        moveDir = Camera.main.transform.TransformDirection(moveDir);
        moveDir.y = 0;
        moveDir.Normalize();

        player.Rigidbody.linearVelocity = new Vector3(moveDir.x * player.MoveSpeed, player.Rigidbody.linearVelocity.y, moveDir.z * player.MoveSpeed);

        if (input != Vector2.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, player.RotationSpeed * Time.fixedDeltaTime);
        }
    }

    public override void Execute()
    {
        base.Execute();

        // ÀÔ·ÂÀÌ ¸ØÃß¸é -> IdleState·Î º¹±Í
        if (player.InputHandler.MoveInput.sqrMagnitude < 0.01f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }
}
