using UnityEngine;

public class PlayerMoveState : PlayerStateBase
{
    public PlayerMoveState(PlayableCharacter player, StateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        player.InputHandler.OnPlayerInput();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void FixedExecute()
    {
        base.FixedExecute();

        Vector2 input = player.InputHandler.MoveInput;

        // 인풋이 없으면 속도 감속
        if (input.sqrMagnitude < 0.01f)
        {
            player.Rigidbody.linearVelocity = Vector3.Lerp(player.Rigidbody.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 10f);
            return;
        }

        // 카메라 기준 입력 방향 계산
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 입력값에 따른 이동 방향 벡터
        Vector3 targetDir = (camForward * input.y + camRight * input.x).normalized;

        // 이동방향을 보게함
        if (targetDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            player.transform.rotation = Quaternion.Slerp(
                player.transform.rotation,
                targetRotation,
                player.RotationSpeed * Time.fixedDeltaTime
            );
        }

        // 이동속독 적용
        float currentMoveSpeed = GetCurrentMoveSpeed();
        player.Rigidbody.linearVelocity = new Vector3(
            targetDir.x * currentMoveSpeed,
            player.Rigidbody.linearVelocity.y,
            targetDir.z * currentMoveSpeed
        );
    }

    public override void Execute()
    {
        base.Execute();

        Vector2 input = player.InputHandler.MoveInput;

        float animSpeed = 0f;
        if (input.sqrMagnitude > 0.01f)
        {
            if (player.InputHandler.IsWalkInput) animSpeed = 0.5f;
            else if (player.InputHandler.IsSprintInput) animSpeed = 1.5f;
            else animSpeed = 1.0f; // 기본은 달리기
        }

        player.UpdateMoveAnimation(animSpeed);
    }


    private float GetCurrentMoveSpeed()
    {
        if (player.InputHandler.IsWalkInput) return player.MoveSpeed * 0.5f;
        if (player.InputHandler.IsSprintInput) return player.MoveSpeed * 1.5f;
        return player.MoveSpeed;
    }
}
#region 레거시 코드
//using UnityEngine;

//public class PlayerMoveState : PlayerStateBase
//{
//    public PlayerMoveState(PlayableCharacter player, StateMachine stateMachine)
//        : base(player, stateMachine) { }

//    private readonly int HashInputX = Animator.StringToHash("InputX");
//    private readonly int HashInputZ = Animator.StringToHash("InputZ");

//    private float WalkMultiplier = 0.5f;   // Walk 애니메이션 좌표
//    private float RunMultiplier = 1.0f;    // Run 애니메이션 좌표
//    private float SprintMultiplier = 1.5f; // Sprint 애니메이션 좌표




//    // private float WalkSpeed = 2.0f;
//    // private float SprintSpeed = 8.0f;


//    public override void Enter()
//    {
//        base.Enter();

//        player.InputHandler.OnPlayerInput();
//    }

//    public override void Exit()
//    {
//        base.Exit();
//    }

//    public override void FixedExecute()
//    {
//        base.FixedExecute();

//        Vector2 input = player.InputHandler.MoveInput;
//        Vector3 inputDir = new Vector3(input.x, 0, input.y);

//        if (Camera.main != null)
//        {
//            inputDir = Camera.main.transform.TransformDirection(inputDir);
//            inputDir.y = 0;
//            inputDir.Normalize();
//        }

//        float targetSpeed = GetCurrentMoveSpeed();

//        player.Rigidbody.linearVelocity = new Vector3(
//            inputDir.x * targetSpeed,
//            player.Rigidbody.linearVelocity.y,
//            inputDir.z * targetSpeed
//        );

//        if (Camera.main != null)
//        {
//            Vector3 camForward = Camera.main.transform.forward;
//            camForward.y = 0;
//            camForward.Normalize();
//            if (camForward != Vector3.zero)
//            {
//                player.transform.rotation = Quaternion.Slerp(
//                    player.transform.rotation,
//                    Quaternion.LookRotation(camForward),
//                    player.RotationSpeed * Time.fixedDeltaTime
//                );
//            }
//        }
//    }

//    public override void Execute()
//    {
//        base.Execute();

//        Vector2 input = player.InputHandler.MoveInput;
//        CheckSwitchState(input);



//        if (input.sqrMagnitude < 0.01f)
//        {
//            player.Animator.SetFloat(HashInputX, 0, 0.15f, Time.deltaTime);
//            player.Animator.SetFloat(HashInputZ, 0, 0.15f, Time.deltaTime);
//            return;
//        }

//        float speedFactor = GetCurrentSpeedMultiplier();

//        Vector3 inputDir = new Vector3(input.x, 0, input.y).normalized;
//        if (Camera.main != null)
//        {
//            inputDir = Camera.main.transform.TransformDirection(inputDir);
//            inputDir.y = 0;
//            inputDir.Normalize();
//        }

//        Vector3 localDir = player.transform.InverseTransformDirection(inputDir);

//        player.Animator.SetFloat(HashInputX, localDir.x * speedFactor, 0.15f, Time.deltaTime);
//        player.Animator.SetFloat(HashInputZ, localDir.z * speedFactor, 0.15f, Time.deltaTime);
//    }

//    private float GetCurrentSpeedMultiplier()
//    {
//        if (player.InputHandler.IsWalkInput) return WalkMultiplier;
//        if (player.InputHandler.IsSprintInput) return SprintMultiplier;
//        return RunMultiplier;
//    }

//    private float GetCurrentMoveSpeed()
//    {
//        if (player.InputHandler.IsWalkInput) return player.MoveSpeed * 0.5f;
//        if (player.InputHandler.IsSprintInput) return player.MoveSpeed * 1.5f;
//        return player.MoveSpeed;
//    }


//    private void CheckSwitchState(Vector2 input)
//    {
//        if (player.InputHandler.JumpTriggered)
//        {
//            var dir = player.GetMoveDir(input);

//            if (dir == PlayableCharacter.MoveDir.Left || dir == PlayableCharacter.MoveDir.Right)
//            {
//                if (player.CanDodge)
//                {
//                    player.LastDodgeTime = Time.time;
//                    stateMachine.ChangeState(player.DodgeState);
//                }
//                else
//                {
//                    return;
//                }
//            }
//            else
//            {
//                stateMachine.ChangeState(player.JumpState);
//            }
//            return;
//        }

//        if(player.InputHandler.AttackLeftTriggered || player.InputHandler.AttackRightTriggered)
//        {
//            stateMachine.ChangeState(player.AttackState);
//            return;
//        }
//    }
//}
#endregion