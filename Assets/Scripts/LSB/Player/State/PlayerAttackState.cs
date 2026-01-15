using UnityEngine;

public class PlayerAttackState : PlayerStateBase
{
    private readonly int LeftHand = Animator.StringToHash("LeftHand");
    private readonly int RightHand = Animator.StringToHash("RightHand");

    public PlayerAttackState(PlayableCharacter player, StateMachine stateMachine, string animationNum)
        : base(player, stateMachine, animationNum) { }

    private const float _LeftAnimDuration = 0.8f; // 왼손 공격 지속시간
    private const float _RightAnimDuration = 0.8f; // 오른손 공격 지속시간
    private float _stateEnterTime; // 상태 진입시간

    private bool isLeft;

    public override void Enter()
    {
        base.Enter();

        Vector2 input = player.InputHandler.MoveInput;
        player.Rigidbody.linearVelocity = Vector3.zero;
        player.InputHandler.OffPlayerInput();
        _stateEnterTime = Time.time;

        if (player.InputHandler.AttackLeftTriggered)
        {
            isLeft = true;
            player.Animator.SetTrigger(LeftHand);
        }
        else if(player.InputHandler.AttackRightTriggered)
        {
            isLeft = false;
            player.Animator.SetTrigger(RightHand);
        }

        player.MagicSystem.CastMagic(isLeft);
    }

    public override void FixedExecute()
    {
        base.FixedExecute();
    }

    public override void Execute()
    {
        base.Execute();

        if(isLeft)
        {
            if (Time.time >= _stateEnterTime + _LeftAnimDuration)
            {
                player.Rigidbody.linearVelocity = Vector3.zero;
                stateMachine.ChangeState(player.MoveState);
            }
            return;
        }
        else
        {
            if (Time.time >= _stateEnterTime + _RightAnimDuration)
            {
                player.Rigidbody.linearVelocity = Vector3.zero;
                stateMachine.ChangeState(player.MoveState);
            }
            return;
        }
    }
}
