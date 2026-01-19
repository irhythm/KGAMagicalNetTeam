using UnityEngine;

public class PlayerAttackState : PlayerStateBase
{
    public PlayerAttackState(PlayableCharacter player, StateMachine stateMachine)
        : base(player, stateMachine) { }

    private const float AnimDuration = 0.8f;

    private bool _isLeftHand;
    private ActionBase _currentAction;
    private float _finishTime;

    public void Init(bool isLeftHand, ActionBase action)
    {
        this._isLeftHand = isLeftHand;
        this._currentAction = action;
    }

    public override void Enter()
    {
        base.Enter();

        if (_currentAction == null || _currentAction.BaseData == null)
        {
            stateMachine.ChangeState(player.MoveState);
            return;
        }

        if (_isLeftHand)
            player.Animator.SetInteger(player.HashAttackID, 0);
        else
            player.Animator.SetInteger(player.HashAttackID, 1);

        player.Animator.SetTrigger(player.HashAttackTrigger);

        player.MagicSystem.UseAction(_isLeftHand);

        _finishTime = Time.time + AnimDuration;
    }

    public override void Execute()
    {
        base.Execute();

        if (Time.time >= _finishTime)
        {
            stateMachine.ChangeState(player.MoveState);
        }
    }
}