using UnityEngine;

public class PlayerDodgeState : PlayerStateBase
{
    private readonly int JumpType = Animator.StringToHash("JumpType");
    private readonly int JumpTrigger = Animator.StringToHash("JumpTrigger");

    private float _dodgeDuration; // 회피 지속시간
    private float _stateEnterTime; // 상태 진입시간

    public PlayerDodgeState(PlayableCharacter player, StateMachine stateMachine, string animationNum)
        : base(player, stateMachine, animationNum) { }

    public override void Enter()
    {
        base.Enter();

        // 입력 차단하고 시작 시간 초기화
        Vector2 input = player.InputHandler.MoveInput;
        player.InputHandler.OffPlayerInput();
        _stateEnterTime = Time.time;

        // 움직인 방향 계산하고 파라미터 변경
        var dir = player.GetMoveDir(input);
        player.Animator.SetInteger(JumpType, (int)dir);
        player.Animator.SetTrigger(JumpTrigger);

        _dodgeDuration = 0.8f;

        //힘 적용
        Vector3 moveDir = (dir == PlayableCharacter.MoveDir.Left ? -player.transform.right : player.transform.right);
        Vector3 force = moveDir * player.DodgeForce;
        player.Rigidbody.linearVelocity = Vector3.zero;
        player.Rigidbody.AddForce(force, ForceMode.Impulse);
    }

    public override void Execute()
    {
        base.Execute();

        if (Time.time >= _stateEnterTime + _dodgeDuration)
        {
            player.Rigidbody.linearVelocity = Vector3.zero;
            stateMachine.ChangeState(player.MoveState);
        }
    }

    public override void Exit()
    {
        base.Exit(); 
    }
}