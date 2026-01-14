using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;//테스트용

//마법 사용 정보, 경비간 무전 구현할 인스턴스 컴포넌트
//씬 장착, 파괴는 시킴
//코루틴 매니저 체크

public class GuardManager : MonoBehaviourPunCallbacks
{
    public static GuardManager instance;

    [Header("스폰 세팅")]
    public Transform[] spawnPoints;
    [Header("경비 프리팹은 무조건 Resources에 넣어둬야함")]
    public GameObject guardPrefab;
    public float spawnInterval = 60f;

    //타이머
    public float Timer { get; private set; } = 0f;
    public bool IsTimerRunning { get; private set; } = false; //타이머 작동 여부

    //스폰
    private float nextSpawnTargetTime = 60f; //첫 스폰 1분 대기
    private int currentWave = 0;

    public struct MagicNoise
    {
        public Vector3 position;
        public float time;
    }

    private List<MagicNoise> magicNoises = new List<MagicNoise>();
    //타겟 공유 딕셔너리. 중복 방지용 key 액터넘버, val 위치벡터값 
    private Dictionary<int, Vector3> sharedTargets = new Dictionary<int, Vector3>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(CoCleanupNoises());
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            NotifyPlayerTransform();
        }

        if (!IsTimerRunning) return;

        //클라이언트 시간 각자 흐르게(어차피 그거없잖아 그거 난입)
        Timer += Time.deltaTime;
        if (PhotonNetwork.IsMasterClient)
        {
            if (Timer >= nextSpawnTargetTime)
            {
                SpawnWave();
                nextSpawnTargetTime += 60f; //또60초
            }
        }
    }

    //UI 표기
    public string GetFormattedTime()
    {
        if (!IsTimerRunning) return "00:00"; //변신 전에는 00:00

        int minutes = Mathf.FloorToInt(Timer / 60f);
        int seconds = Mathf.FloorToInt(Timer % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    #region 트리거
    //플레이어가 마법사로 변신했을 때 (딱 1번 호출) (신빈님)
    public void NotifyPlayerTransform()
    {
        photonView.RPC("RpcStartTimer", RpcTarget.All);
    }

    //시민이 탈출했을 때
    public void NotifyCitizenEscape()
    {
        photonView.RPC("RpcModifyTimer", RpcTarget.All, 10f);
    }

    #endregion


    //노이즈 데이터 삭제, 최적화 필요 시 5초마다 삭제예정
    IEnumerator CoCleanupNoises()
    {
        var waitOneSec = CoroutineManager.waitForSeconds(1.0f);
        while (true)
        {
            yield return waitOneSec;
            if (magicNoises.Count > 0)
            {
                //체크용
                float expireTime = Time.time - 60f;
                //역순으로 돌기, 만료 시 삭제
                for (int i = magicNoises.Count - 1; i >= 0; i--)
                {
                    if (magicNoises[i].time < expireTime)
                    {
                        magicNoises.RemoveAt(i);
                    }
                }
            }
        }
    }
    //룸오브젝트로 생성하여 마스터클라 나가도 경비병 남아있게
    //시민은 배치해둘거같음 그냥
    private void SpawnWave()
    {
        currentWave++;
        //2제곱
        int calcCount = (int)Mathf.Pow(2, currentWave);
        int spawnCountPerPoint = Mathf.Min(calcCount, 6);
        foreach (Transform point in spawnPoints)
        {
            for (int i = 0; i < spawnCountPerPoint; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-6f, 6f), 0, Random.Range(-6f, 6f)); //근처 랜덤 소환
                Vector3 spawnPos = point.position + offset;
                if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out UnityEngine.AI.NavMeshHit hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    PhotonNetwork.InstantiateRoomObject(
guardPrefab.name,
point.position + offset,
Quaternion.identity); //룸 오브젝트로 소환
                }

            }
        }
        //웨이브 실행할 때마다 시간 오차 맞추기
        photonView.RPC("RpcSyncTimer", RpcTarget.Others, currentWave, Timer);
    }

    //각 경비AI가 호출
    //타겟 업승면 null 반환하려고 Vector3? 
    public Vector3? GetBestTargetPosition(Vector3 guardPos)
    {
        //1순위 적 위치
        if (sharedTargets.Count > 0)
        {
            Vector3 bestPos = Vector3.zero;
            float minDistSqr = float.MaxValue;
            bool found = false;
            //위치 확인
            foreach (var kvp in sharedTargets)
            {
                float d = (kvp.Value - guardPos).sqrMagnitude;//제곱
                if (d < minDistSqr)//더가까움?
                {
                    minDistSqr = d;
                    bestPos = kvp.Value;
                    found = true;//찾았다
                }
            }
            if (found) return bestPos;//마법 소음 무시하고 반환
        }

        //2순위 마법 발동위치
        if (magicNoises.Count > 0)
        {
            Vector3 bestPos = Vector3.zero;
            float minDistSqr = float.MaxValue;
            bool found = false;

            foreach (var noise in magicNoises)
            {
                float d = (noise.position - guardPos).sqrMagnitude; //똑같이
                if (d < minDistSqr)
                {
                    minDistSqr = d;
                    bestPos = noise.position;
                    found = true;
                }
            }
            if (found) return bestPos; //리턴
        }
        return null;
    }

    //딕셔너리 갱신
    private void UpdateEnemyInfo(int actorNumber, Vector3 pos)
    {
        if (sharedTargets.ContainsKey(actorNumber)) //딕셔너리에 있으면
            sharedTargets[actorNumber] = pos; //위치만 수정
        else
            sharedTargets.Add(actorNumber, pos); //업승면 추가
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (newMasterClient.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            if (IsTimerRunning)
            {
                nextSpawnTargetTime = Mathf.Ceil(Timer / 60f) * 60f;
                if (nextSpawnTargetTime <= Timer) nextSpawnTargetTime += 60f;
            }
        }
    }


    //마법용(신빈님), 발동 시점에 호출
    public void RegisterMagicNoise(Vector3 pos)
    {
        photonView.RPC("RpcAddMagicNoise", RpcTarget.All, pos);
    }

    //경비 AI용, 플레이어 조우 시점에 호출
    public void ReportEnemy(int actorNumber, Vector3 pos)
    {
        UpdateEnemyInfo(actorNumber, pos);
        photonView.RPC("RpcUpdateEnemyInfo", RpcTarget.Others, actorNumber, pos);
    }

    #region RPC 정리

    [PunRPC]
    void RpcAddMagicNoise(Vector3 pos)
    {
        magicNoises.Add(new MagicNoise { position = pos, time = Time.time }); //포지션, 시간(청소용)
    }

    [PunRPC]
    void RpcUpdateEnemyInfo(int actorNumber, Vector3 pos)
    {
        UpdateEnemyInfo(actorNumber, pos); //무전받고 갱신
    }

    [PunRPC]
    void RpcSyncTimer(int wave, float masterTime)
    {
        currentWave = wave;

        //시간 오차 3초 이상 발생 시 방장 시간으로 강제 동기화
        if (Mathf.Abs(Timer - masterTime) > 3f)
        {
            Timer = masterTime;
        }
    }

    //타이머 시작 명령
    [PunRPC]
    void RpcStartTimer()
    {
        if (IsTimerRunning) return;
        Debug.Log("플레이어 변신 체크, 타이머 가동");
        IsTimerRunning = true;
        Timer = 0f;

        //초기화= 첫 스폰까지 60초
        nextSpawnTargetTime = 60f;
        currentWave = 0;
    }

    //탈출 시 시간 갱신 명령 +10
    [PunRPC]
    void RpcModifyTimer(float amount)
    {
        Timer += amount;
        Debug.Log("타이머 10초 증가");
    }

    #endregion
}
