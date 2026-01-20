using UnityEngine;
using UnityEngine.AI;
using BzKovSoft.RagdollTemplate.Scripts.Charachter;


//어댑터 패턴에서의 어댑터
//상태 제어 매핑 및 데이터 변환 작업 수행
//CharacterVelocity 요청 → Adaptee의 agent.velocity or 리짓바디 속도를 반환하도록 연결

public class HumanoidRagdollCharacter : MonoBehaviour, IBzRagdollCharacter
{
    private NavMeshAgent agent;
    private Rigidbody rigid;
    private Collider rootCollider;
    private Rigidbody[] allRigidbodies;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rigid = GetComponent<Rigidbody>();
        rootCollider = GetComponent<Collider>();
        allRigidbodies = GetComponentsInChildren<Rigidbody>();

        if (rootCollider != null)
        {
            Collider[] childColliders = GetComponentsInChildren<Collider>();
            foreach (Collider child in childColliders)
            {
                //Root 아니면 충돌 무시 설정
                if (child != rootCollider)
                {
                    Physics.IgnoreCollision(rootCollider, child, true);
                }
            }
        }
    }

    //현재 속도 반환(관성 유지), 데이터 변환 과정
    public Vector3 CharacterVelocity
    {
        get
        {
            //네비매쉬 가동중이면 네비 속도 가져오기
            if (agent != null && agent.enabled) return agent.velocity;

            //물리 운전중이면 물리의 속도값
            if (rigid != null) return rigid.linearVelocity;

            return Vector3.zero;
        }
    }

    //상태 제어 매핑
    //클라이언트(Bz)의 CharacterEnable 요청하면 -> 어댑티는 agent.enable 관리
    public void CharacterEnable(bool enable)
    {
        //false = 레그돌 on = 피격 -> AI Off
        //true = 래그돌 off = 기상 -> AI On

        if (agent != null)
        {
            //켤 땐 컨트롤러가
            if (!enable) agent.enabled = false;
        }


        if (allRigidbodies != null)
        {
            foreach (var rb in allRigidbodies)
            {
                if (rb != rigid) //몸통 제외
                {
                    rb.useGravity = !enable; //중력 켜기

                    if (!enable)
                    {
                        rb.detectCollisions = true;
                        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                        rb.WakeUp();
                    }
                }
            }
        }

    }
}
