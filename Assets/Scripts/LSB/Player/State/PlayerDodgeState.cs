using UnityEngine;

public class PlayerDodgeState : PlayerStateBase
{
    private float _dodgeDuration = 0.9f;
    private float _stateEnterTime;

    public PlayerDodgeState(PlayableCharacter player, StateMachine stateMachine, string animationNum)
        : base(player, stateMachine, animationNum) { }

    public override void Enter()
    {
        base.Enter();

        Vector2 input = player.InputHandler.MoveInput;
        Vector3 dodgeDir = input.x >= 0 ? Camera.main.transform.right : -Camera.main.transform.right;

        player.InputHandler.OffPlayerInput();
        _stateEnterTime = Time.time;

        if (dodgeDir != Vector3.zero)
        {
            player.Animator.transform.rotation = Quaternion.LookRotation(dodgeDir);
        }

        if (player.Animator != null)
            player.Animator.applyRootMotion = true;

        player.Rigidbody.linearVelocity = Vector3.zero;
        player.Rigidbody.AddForce(dodgeDir * player.DodgeForce, ForceMode.Impulse);
    }

    public override void Execute()
    {
        base.Execute();

        // 지속 시간이 지나면 이동 상태로 복귀
        if (Time.time >= _stateEnterTime + _dodgeDuration)
        {
            player.Rigidbody.linearVelocity = Vector3.zero;
            stateMachine.ChangeState(player.MoveState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (player.Animator != null)
            player.Animator.applyRootMotion = false;

        player.Animator.transform.localRotation = Quaternion.identity;
        player.Animator.transform.localPosition = Vector3.zero;

        player.InputHandler.OnPlayerInput();
    }
}