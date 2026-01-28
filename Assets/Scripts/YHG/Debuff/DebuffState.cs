using UnityEngine;
using UnityEngine.AI;

public class DebuffState : AIStateBase
{
    public DebuffInfo CurrentInfo { get; private set; }

    private float timer = 0f;
    private float wanderTimer = 0f;

    //복구용 데이터
    private float originalSpeed;

    //동적모델 담는 변수
    private GameObject currentVisualInstance;

    public DebuffState(BaseAI ai, StateMachine machine, DebuffInfo info)
        : base(ai, machine, BaseAI.AIStateID.Debuff)
    {
        CurrentInfo = info;
    }

    public override void Enter()
    {
        base.Enter();
        if (ai.Agent == null || !ai.Agent.isOnNavMesh) return;

        timer = 0f;
        originalSpeed = ai.Agent.speed;

        switch (CurrentInfo.Type)
        {
            case DebuffType.Stun:
                ai.Agent.isStopped = true;
                ai.Agent.velocity = Vector3.zero;
                if (ai.Anim) ai.Anim.speed = 0f;

                if (CurrentInfo.VisualPrefab != null)
                {
                    currentVisualInstance = Object.Instantiate(CurrentInfo.VisualPrefab);
                }
                break;
                //추가적인 로직은 테스트 후 추가
            case DebuffType.Execution:
                ai.Agent.isStopped = true;
                ai.Agent.velocity = Vector3.zero;
                break;

            case DebuffType.Polymorph:
                ai.Agent.isStopped = false;
                ai.Agent.speed = ai.moveSpeed * CurrentInfo.Value;

                if(ai.humanModel) ai.humanModel.SetActive(false);
                if (CurrentInfo.VisualPrefab != null)
                {
                    currentVisualInstance = Object.Instantiate (CurrentInfo.VisualPrefab);

                    currentVisualInstance.transform.localPosition = Vector3.zero;
                    currentVisualInstance.transform.localRotation = Quaternion.identity;
                }
                break;
            case DebuffType.Slow:
                ai.Agent.speed = ai.moveSpeed * CurrentInfo.Value;
                break;
        }
    }

    public override void Execute()
    {
        if (ai.Agent == null || !ai.Agent.isActiveAndEnabled || !ai.Agent.isOnNavMesh) return;

        timer += Time.deltaTime;
        if (timer >= CurrentInfo.Duration)
        {
            ReturnToOriginalState();
            return;
        }
        switch (CurrentInfo.Type)
        {
            case DebuffType.Polymorph:
                WanderBehavior();
                break;
        }
    }
    //public override void Exit()
    //{
    //    base.Exit();

    //    if (ai.Agent != null && ai.Agent.isOnNavMesh && ai.Agent.isActiveAndEnabled)
    //    {
    //        ai.Agent.speed = ai.moveSpeed;
    //        ai.Agent.isStopped = false;
    //    }
    //    if (ai.Anim) ai.Anim.speed = 1f;

    //    switch
    //}

    //양 변이 시 랜덤 배회 로직
    private void WanderBehavior()
    {
        wanderTimer += Time.deltaTime;
        if (wanderTimer > 2.0f) //2초마다 방향 전환
        {
            wanderTimer = 0f;
            Vector3 randomPoint = ai.transform.position + Random.insideUnitSphere * 3.0f;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 3.0f, NavMesh.AllAreas))
            {
                ai.Agent.SetDestination(hit.position);
            }
        }
    }

    //원래 하던 일 복귀
    private void ReturnToOriginalState()
    {
        if (ai is GarrisonGuardAI garrison)
        {
            stateMachine.ChangeState(new GarrisonPatrolState(garrison, stateMachine));
        }
        else if (ai is GuardAI guard)
        {
            stateMachine.ChangeState(new GuardPatrolState(guard, stateMachine));
        }
        else if (ai is CitizenAI citizen)
        {
            stateMachine.ChangeState(new CitizenPatrolState(citizen, stateMachine));
        }
    }

}
