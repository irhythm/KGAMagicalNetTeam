using UnityEngine;

public class PlayerDodgeState : PlayerStateBase
{
    private float _dodgeDuration = 0.2f;
    private float _stateEnterTime;

    public PlayerDodgeState(PlayableCharacter player, StateMachine stateMachine, string animationNum)
        : base(player, stateMachine, animationNum) { }

    public override void Enter()
    {
        base.Enter();

        player.InputHandler.OffPlayerInput();
        _stateEnterTime = Time.time;

        Vector2 input = player.InputHandler.MoveInput;
        Vector3 dodgeDir = Vector3.zero;

        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            dodgeDir = (camForward * input.y + camRight * input.x).normalized;
        }
        else
        {
            dodgeDir = player.transform.forward;
        }

        if (dodgeDir != Vector3.zero)
        {
            player.transform.rotation = Quaternion.LookRotation(dodgeDir);
        }

        player.Animator.SetInteger(player.HashDodgeType, 0);

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

        player.InputHandler.OnPlayerInput();
    }
}