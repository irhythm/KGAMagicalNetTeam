using UnityEngine;
using Photon.Pun;

//주둔기 경비병 AI
public class GarrisonGuardAI : GuardAI
{
    [Header("주둔지 설정")]
    public float fovAngle = 110f;       //시야각
    public float maxChaseDist = 30f;    //주둔지 중심부터 최대 거리
    public float heightThreshold = 2.5f;//시야높이임계점

    private Vector3 anchorPoint;
    public Vector3 AnchorPoint => anchorPoint;

    [Header("시야 체크용")]
    public Transform eyeTransform;

    //소음/신고 감지 상태 저장용 (State에서 갖다 씀)
    public Vector3 lastNoisePos;
    public bool hasNoiseDetected = false;


    //오버라이드
    protected override void SetInitialState()
    {
        // ChangeState(new GarrisonPatrolState(this, stateMachine));
    }

    //4단계 시야 필터링
    //거리  / 높이 / 각도 / 레이
    //오버랩/ 임계 / 앵글 / 레이

    //public override bool CheckEnemyNearby()
    //{

    //}


    ////멀티 레이캐스트 로직
    //private bool CanSeeTarget(Transform target)
    //{

    //}

    //소음 감지(주둔지 전용)
    //주둔지 체크(앵커포인트 기준 maxChasDIst 범위만)
    public override void OnHearNoise(Vector3 noisePos)
    {

    }

    //내부망 신고(가드매니저 안 씀 애는, 오버랩스피어로 동료만 호출)
    public override void ReportEnemy(Vector3 pos)
    {

    }

    //동료의 신고를 받는 함수(호출되면 받기)
    public void ReceiveAlert(Vector3 enemyPos)
    {

    }

    //기즈모
    private void OnDrawGizmosSelected()
    {
        //감지범위, 주둔지 범위, 시야각 3개
    }

}
