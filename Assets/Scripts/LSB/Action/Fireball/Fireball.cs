using Photon.Pun;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// 파이어볼 투사체 로직입니다.
/// 충돌 시 네트워크 RPC로 폭발을 알리고, 범위 내 오브젝트에 물리력을 가함
/// </summary>
public class Fireball : MonoBehaviourPun
{
    [Header("Settings")]
    [SerializeField] private FireballSO fireballData;

    private int shooterActorNumber;
    private bool hasExploded = false;

    private float currentTimer = 0f;

    Rigidbody rb;

    private void Start()
    {
        //2601213 양현용 : 파이어볼 소환 사운드 추가
        SoundManager.Instance.PlaySFX(fireballData.magicSound, 1f, 100f, gameObject.transform.position);

        rb = GetComponent<Rigidbody>();
        ApplyVelocity();
    }

    void FixedUpdate()
    {
        currentTimer += Time.fixedDeltaTime;

        ApplyVelocity();
    }

    private bool isDestroyed = false;

    public void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    public void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene current, Scene next)
    {
        if (isDestroyed || !photonView.IsMine) return;

        isDestroyed = true;
        PhotonNetwork.Destroy(gameObject);
    }

    private void ApplyVelocity()
    {
        if (rb == null) return;

        float progress = Mathf.Clamp01(currentTimer / fireballData.accelerationTime);
        float currentMul = Mathf.Lerp(fireballData.startSpeedMul, fireballData.maxSpeedMul, progress);

        rb.linearVelocity = transform.forward * (fireballData.speed * currentMul);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        // 발사한 본인에게는 충돌하지 않음
        PhotonView targetView = other.GetComponent<PhotonView>();
        if (targetView != null && targetView.OwnerActorNr == shooterActorNumber)
        {
            if(other.CompareTag("Player"))
                return;
        }

        if (hasExploded) return;
        hasExploded = true;

        // 모든 클라이언트에서 폭발 실행
        photonView.RPC(nameof(RPC_ExplodeProcess), RpcTarget.All, transform.position);
        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    private void RPC_ExplodeProcess(Vector3 explosionPos)
    {
        //2601213 양현용 : 파이어볼 소환 사운드 추가
        SoundManager.Instance.PlaySFX(fireballData.explosionSound, 1f, 100f, gameObject.transform.position);

        if (fireballData.explosionEffectPrefab != null)
            Instantiate(fireballData.explosionEffectPrefab, explosionPos, Quaternion.identity);

        // 폭발 범위 내의 모든 콜라이더 감지
        Collider[] colliders = Physics.OverlapSphere(explosionPos, fireballData.radius, fireballData.explosionLayer);

        foreach (Collider hit in colliders)
        {
            IExplosion targetComponent = hit.GetComponent<IExplosion>();
            if (targetComponent == null) targetComponent = hit.GetComponentInParent<IExplosion>();

            if (targetComponent != null)
            {
                targetComponent.OnExplosion(explosionPos, fireballData, shooterActorNumber);
            }
            else
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(fireballData.knockbackForce, explosionPos, fireballData.radius, fireballData.forceUpward, ForceMode.Impulse);
                }
            }
        }
    }

    

    public void SetShooterActorNumber(int actorNumber)
    {
        shooterActorNumber = actorNumber;
    }
}