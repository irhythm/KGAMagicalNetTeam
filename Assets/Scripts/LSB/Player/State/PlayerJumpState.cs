using UnityEngine;

public class PlayerJumpState : PlayerStateBase
{
    private readonly int JumpType = Animator.StringToHash("JumpType");
    private readonly int JumpTrigger = Animator.StringToHash("JumpTrigger");
    private readonly int VerticalVelue = Animator.StringToHash("VerticalVelocity");

    private float _initJumpTime;

    public PlayerJumpState(PlayableCharacter player, StateMachine stateMachine, string animationNum)
        : base(player, stateMachine, animationNum) { }

    public override void Enter()
    {
        base.Enter();

        // 입력차단
        Vector2 input = player.InputHandler.MoveInput;
        player.InputHandler.OffPlayerInput(); 

        // 애니매이션 방향 설정
        var dir = player.GetMoveDir(input);
        player.Animator.SetInteger(JumpType, (int)dir);
        player.Animator.SetTrigger(JumpTrigger);

        // 점프 방향 초기화
        Vector3 jumpDir = Vector3.zero;

        // 플레이어 기준으로 점프 방향 결정
        if (input.sqrMagnitude > 0.01f)
        {
            // 카메라 기준이 아닌 캐릭터 기준 좌표계로 변환하여 점프 방향 결정
            Vector3 forward = player.transform.forward * input.y;
            Vector3 right = player.transform.right * input.x;
            jumpDir = (forward + right).normalized;
        }

        // 점프 방향에 윗방향으로 힘 추가
        Vector3 force = jumpDir + Vector3.up * player.JumpForce;

        //player.Rigidbody.linearVelocity = Vector3.zero;
        player.Rigidbody.AddForce(force, ForceMode.Impulse);


        _initJumpTime = Time.time + 0.1f;
    }

    public override void FixedExecute()
    {
        base.FixedExecute();

        // 카메라 방향 바라보게
        if (Camera.main != null)
        {
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0; // 수직 값은 무시
            camForward.Normalize();

            if (camForward != Vector3.zero)
            {
                // 부드럽게 회전
                player.transform.rotation = Quaternion.Slerp(
                    player.transform.rotation,
                    Quaternion.LookRotation(camForward),
                    player.RotationSpeed * Time.fixedDeltaTime
                );
            }
        }

        // y축 이동하고 있고 인풋 땟을 때 플레이어 y가족 점차 감소
        if (player.Rigidbody.linearVelocity.y > 0 && !player.InputHandler.JumpButtonHeld)
        {
            Vector3 vel = player.Rigidbody.linearVelocity;
            vel.y *= 0.8f;
            player.Rigidbody.linearVelocity = vel;
        }

        // y변화가 없고 0.1초가 지났으며 그라운드 체크가 된다면 상태 전환
        if (Time.time > _initJumpTime && player.CheckIsGrounded() && player.Rigidbody.linearVelocity.y <= 0.1f)
        {
            stateMachine.ChangeState(player.MoveState);
        }

        // 낙하 애니메이션 블렌딩
        player.Animator.SetFloat(VerticalVelue, player.Rigidbody.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
        player.InputHandler.OnPlayerInput();
    }
}