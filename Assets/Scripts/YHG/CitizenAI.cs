using UnityEngine;
using Photon.Pun;

//시민 유닛의 각 상태에 필요한 변수 정리, 센서 기능만 등록

public class CitizenAI : BaseAI
{
    [Header("시민 설정")]
    public float patrolRange = 5f;
    public float detectRadius = 15f;
    public float runSpeed = 6.0f;
    public LayerMask playerLayer;

    private bool isEscaped = false;


    private Vector3 startPos;
    //상태가 사용할 변수
    public Transform detectedPlayer {  get; private set; }


    //시작지점 초기화
    protected override void Awake()
    {
      base.Awake();  
        startPos = transform.position;
    }
    //초기상태 지정
    protected override void SetInitialState()
    {
        //시작은 패트롤
       ChangeState(new CitizenPatrolState(this, stateMachine));
    }

    //상태클래스용 함수들

    //순찰 다음 이동 지점 계산, 일단 z축으로만
    public Vector3 GetPatrolPoint(bool forward)
    {
        float dir = forward ? 1f : -1f;
        //정면 기준으로 앞뒤 거리 계산
        return startPos + (transform.forward * dir * patrolRange); //패트롤거리만큼
    }

    //근처 플레이어 찾기
    //탐지용 배열
    private Collider[] connectionBuffer = new Collider[1];
    public bool CheckPlayerNearby()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectRadius, connectionBuffer ,playerLayer);
        if (count > 0)
        {
            detectedPlayer = connectionBuffer[0].transform;
            return true;
        }
        detectedPlayer = null;
        return false;
    }
    //Run, 가장 가까운 탈출구, 태그로 지정해두기, 추후 최적화?
    public Vector3 GetNearestExit()
    {
        GameObject[] exits = GameObject.FindGameObjectsWithTag("ExitPoint");

        //예외처리 일단 앞으로 50m 달리기
        if (exits.Length == 0)
        {
            Debug.LogWarning("ExitPoint 없음 돌진");
            return transform.position + transform.forward * 50f;
        }

        GameObject nearestObj = null;
        float minDistSqr = float.MaxValue;
        Vector3 currentPos = transform.position;

       //돌려잇
        foreach (GameObject exit in exits)
        {
            //거리제곱으로 계산
            float distSqr = (exit.transform.position - currentPos).sqrMagnitude;

            if (distSqr < minDistSqr)
            {
                minDistSqr = distSqr;
                nearestObj = exit;
            }
        }
        return nearestObj.transform.position;
    }

    //틸출 성공
    public void OnEscapeSuccess()
    {
        if (isEscaped) return;
        isEscaped = true;
        Debug.Log("시민 탈출 성공! (경비 스폰 시간 단축 페널티)");
        if (!PhotonNetwork.IsMasterClient) return;

        Agent.enabled = false; //길찾기 끄기
        this.enabled = false; //AI 끄기

        //GameManager 등 시간재는 애 -10초 
        if (GuardManager.instance != null)
        {
            GuardManager.instance.NotifyCitizenEscape();
        }

        //오브젝트 파괴는 방장 권한으로
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}