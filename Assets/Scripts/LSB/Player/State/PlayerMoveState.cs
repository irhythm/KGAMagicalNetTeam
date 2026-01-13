using UnityEngine;

public class PlayerMoveState : PlayerStateBase
{
    public PlayerMoveState(PlayableCharacter player, StateMachine stateMachine)
        : base(player, stateMachine, "IsMove") { }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
        player.Animator.SetFloat(player.HashInputX, 0);
        player.Animator.SetFloat(player.HashInputZ, 0);
    }

    public override void FixedExecute()
    {
        base.FixedExecute();

        Vector2 input = player.InputHandler.MoveInput;
        Vector3 inputDir = new Vector3(input.x, 0, input.y);

        if (Camera.main != null)
        {
            inputDir = Camera.main.transform.TransformDirection(inputDir);
            inputDir.y = 0;
            inputDir.Normalize();
        }

        player.Rigidbody.linearVelocity = new Vector3(
            inputDir.x * player.MoveSpeed,
            player.Rigidbody.linearVelocity.y,
            inputDir.z * player.MoveSpeed
        );

        if (Camera.main != null)
        {
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;
            camForward.Normalize();

            if (camForward != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(camForward);

                player.transform.rotation = Quaternion.Slerp(
                    player.transform.rotation,
                    targetRotation,
                    player.RotationSpeed * Time.fixedDeltaTime
                );
            }
        }
    }

    public override void Execute()
    {
        base.Execute();

        Vector2 input = player.InputHandler.MoveInput;

        if (input.sqrMagnitude < 0.01f)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        UpdateBlendTree();
    }

    private void UpdateBlendTree()
    {
        Vector3 worldVelocity = player.Rigidbody.linearVelocity;
        worldVelocity.y = 0;

        if (worldVelocity.magnitude < 0.1f)
        {
            player.Animator.SetFloat(player.HashInputX, 0);
            player.Animator.SetFloat(player.HashInputZ, 0);
            return;
        }

        Vector3 localVelocity = player.transform.InverseTransformDirection(worldVelocity.normalized);

        player.Animator.SetFloat(player.HashInputX, localVelocity.x, player.AnimDampTime, Time.deltaTime);
        player.Animator.SetFloat(player.HashInputZ, localVelocity.z, player.AnimDampTime, Time.deltaTime);
    }
}