using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] GameObject beam;
    public float health = 1f;

    bool isFiring;
    InputAction attackAction;
    public static GameObject LocalPlayerInstance;

    private void Awake()
    {
        if (beam != null)
        {
            beam.SetActive(false);
        }
        attackAction = InputSystem.actions["Attack"];//빠르게 내장된거 사용
    }
    private void Start()
    {
        CameraWork cameraWork=gameObject.GetComponent<CameraWork>();
        if (photonView.IsMine)
        {
            cameraWork.OnStartFollowing();
        }
    }
    public override void OnEnable()
    {
        base.OnEnable();
        if (!photonView.IsMine) // 지금 스크립트를 행하는 포톤뷰가 내 거가 아닐 경우, 내 거만 조작하도록
        {
            return;
        }
        attackAction.Enable();//글로벌 인풋은 자동으로 켜져있긴 함
        attackAction.performed += OnAttack;
        attackAction.canceled += OnAttackCancel;
    }
    public override void OnDisable()
    {
        if (photonView.IsMine) // 지금 스크립트를 행하는 포톤뷰가 내 거가 아닐 경우
        {
            attackAction.performed -= OnAttack;
            attackAction.canceled -= OnAttackCancel;
            attackAction.Disable();
        }

        base.OnDisable();
    }

    void OnAttack(InputAction.CallbackContext ctx)
    {
        isFiring = true;
        UpdateBeamState();
    }

    void OnAttackCancel(InputAction.CallbackContext ctx)
    {
        isFiring = false;
        UpdateBeamState();
    }

    void UpdateBeamState()
    {
        if (beam != null && beam.activeSelf != isFiring)
        {
            beam.SetActive(isFiring);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //내거에만 체력이 닳도록
        if (!photonView.IsMine)
        {
            return;
        }
        if(!other.name.Contains("Beam"))
        {
            return;
        }
        if (other.gameObject == beam)
            return;
        //if(other.CompareTag("Beam"))
            health -= 0.1f;
        CheckDeath();
    }
    private void OnTriggerStay(Collider other)
    {
        //내거에만 체력이 닳도록
        if (!photonView.IsMine)
        {
            return;
        }
        if (!other.name.Contains("Beam"))
        {
            return;
        }
        if (other.gameObject == beam)
            return;
        //if(other.CompareTag("Beam"))
        health -= 0.1f*Time.deltaTime;
        CheckDeath();
    }

    void CheckDeath()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        if ((health<=0))
        {
            GameManager.Instance.LeaveRoom();
            health = 1f;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //동기화시켰으면 하는 메서드 연동 <- 여기다가 npc ai 넣으면 호스트가 실행한 ai에 따라 모든 클라이언트의 npc 움직임이 연동 가능할까?
        if (stream.IsWriting)
        {
            stream.SendNext(isFiring);//자료형 없이 단순 이진수로 보내버림
            stream.SendNext(health);

        }
        else
        {
            this.isFiring = (bool)stream.ReceiveNext();//다시 언박싱 해줘야 함
            this.health = (float)stream.ReceiveNext();//변수만 계속 바뀌고 있음 근데 변수가 바뀌었다고 뭔가 하라고 시지는 없음

            UpdateBeamState();
        }
    }
}
