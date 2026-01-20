using UnityEngine;

public class GuardAttackState : AIStateBase
{
    private GuardAI guard;
    //최적화_ 공격 끝나고 다시 추격 할 거리(사거리비례)
    private float sqrStopAttackRange;

    //공격 중엔 못 움직이게 설정
    private float attackMotionDuration = 2.0f;

    public GuardAttackState(BaseAI ai, StateMachine stateMachine) : base(ai, stateMachine, BaseAI.AIStateID.Attack)
    {
        guard = ai as GuardAI;

        float bufferRange = guard.attackRange + 1f;
        sqrStopAttackRange = bufferRange * bufferRange;
    }

    public override void Enter()
    {
        base.Enter();

        if (guard.Agent != null && guard.Agent.isOnNavMesh)
        {
            guard.Agent.ResetPath();
            guard.Agent.isStopped = true; //이동정지
            guard.Agent.velocity = Vector3.zero; //미끄럼방지

            //네비게이션 트랜스폼 이동 정지
            guard.Agent.updatePosition = false;
            guard.Agent.updateRotation = false;

            //루트모션 쓰면
            if (guard.useRootMotion)
            {
                //애니메이션이 트랜스폼 직접 밀어버리게
                guard.Anim.applyRootMotion = true;
            }
            LookAtTarget(true);
            Attack();
        }
    }
    //상태 탈출 시 다시 true;
    public override void Exit()
    {
        base.Exit();
        if (guard.Agent != null)
        {
            if (guard.useRootMotion)
            {
                guard.Agent.Warp(guard.transform.position);
                guard.Anim.applyRootMotion = false;
            }
            guard.Agent.updatePosition = true;
            guard.Agent.updateRotation = true;
            guard.Agent.isStopped = false;
        }
    }
    public override void Execute()
    {
        //타겟 죽거나 사라지면 순찰
        if (guard.targetPlayer == null)
        {
            stateMachine.ChangeState(new GuardPatrolState(guard, stateMachine));
            return;
        }

        if (guard.Agent != null && guard.Agent.isOnNavMesh)
        {
            guard.Agent.velocity = Vector3.zero;
        }

        //어택애니메이션중이면 체이스 x
        bool isPlayingAttackAnim = (Time.time < guard.lastAttackTime + attackMotionDuration);

        if (!isPlayingAttackAnim)
        {
            //제곱 사용
            float sqrDist = (guard.targetPlayer.position - guard.transform.position).sqrMagnitude;

            if (sqrDist > sqrStopAttackRange)
            {
                //적 멀어짐, 추격 시작
                stateMachine.ChangeState(new GuardChaseState(guard, stateMachine));
                return;
            }
        }



        if (guard.useRootMotion == false)
        {
            LookAtTarget(false);
        }

        //쿨타임 체크 (애니메이션이랑 맞춰야함)
        if (Time.time >= guard.lastAttackTime + guard.attackCooldown)
        {
            Attack();
        }
    }

    //타겟 즉시 보기, lerp 두 개 중 정하도록
    private void LookAtTarget(bool isInstant)
    {
        if (guard.targetPlayer == null) return;

        Vector3 dir = guard.targetPlayer.position - guard.transform.position;
        dir.y = 0; //적이 지붕에 있어도 정면보기 (추후 계단 등 리팩토링)

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            //경비병마다 공격 모션이 달라서 오프셋 따라 방향 비틀어주기
            Quaternion fixRot = Quaternion.Euler(0, guard.attackRotOffset, 0);
            Quaternion finalRot = targetRot * fixRot;
            if (isInstant)
            {
                guard.transform.rotation = finalRot;
            }
            else
            {
                guard.transform.rotation = Quaternion.Slerp(
                guard.transform.rotation,
                targetRot * fixRot,
                Time.deltaTime * guard.rotSpeed * 5f);
            }


        }
    }
    private void Attack()
    {
        guard.lastAttackTime = Time.time; //쿨타임 리셋

        //실제 데미지 판정은 가드AI가
        guard.AttackTarget();
    }
}
