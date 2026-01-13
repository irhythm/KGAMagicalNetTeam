using UnityEngine;

public class PlayerMoveState : PlayerStateBase
{
    public PlayerMoveState(PlayableCharacter player, StateMachine stateMachine)
        : base(player, stateMachine, "IsMove") { }

    private float WalkMultiplier = 0.5f;   // Walk 애니메이션 좌표
    private float RunMultiplier = 1.0f;    // Run 애니메이션 좌표
    private float SprintMultiplier = 1.5f; // Sprint 애니메이션 좌표

    // private float WalkSpeed = 2.0f;
    // private float SprintSpeed = 8.0f;


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

        float targetSpeed = GetCurrentMoveSpeed();

        player.Rigidbody.linearVelocity = new Vector3(
            inputDir.x * targetSpeed,
            player.Rigidbody.linearVelocity.y,
            inputDir.z * targetSpeed
        );

        if (Camera.main != null)
        {
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;
            camForward.Normalize();
            if (camForward != Vector3.zero)
            {
                player.transform.rotation = Quaternion.Slerp(
                    player.transform.rotation,
                    Quaternion.LookRotation(camForward),
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
            player.Animator.SetFloat(player.HashInputX, 0, 0.15f, Time.deltaTime);
            player.Animator.SetFloat(player.HashInputZ, 0, 0.15f, Time.deltaTime);
            return;
        }

        float speedFactor = GetCurrentSpeedMultiplier();

        Vector3 inputDir = new Vector3(input.x, 0, input.y).normalized;
        if (Camera.main != null)
        {
            inputDir = Camera.main.transform.TransformDirection(inputDir);
            inputDir.y = 0;
            inputDir.Normalize();
        }

        Vector3 localDir = player.transform.InverseTransformDirection(inputDir);

        player.Animator.SetFloat(player.HashInputX, localDir.x * speedFactor, 0.15f, Time.deltaTime);
        player.Animator.SetFloat(player.HashInputZ, localDir.z * speedFactor, 0.15f, Time.deltaTime);
    }

    private float GetCurrentSpeedMultiplier()
    {
        if (player.InputHandler.IsWalkInput) return WalkMultiplier;
        if (player.InputHandler.IsSprintInput) return SprintMultiplier;
        return RunMultiplier;
    }

    private float GetCurrentMoveSpeed()
    {
        if (player.InputHandler.IsWalkInput) return player.MoveSpeed * 0.5f;
        if (player.InputHandler.IsSprintInput) return player.MoveSpeed * 1.5f;
        return player.MoveSpeed;
    }
}