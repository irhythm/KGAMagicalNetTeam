using Photon.Pun;
using System.Collections.Generic;
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
    int fryingPanLayer;

    private void Start()
    {
        fryingPanLayer = LayerMask.NameToLayer("FryingPan");
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * fireballData.speed;
        }
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
        if (fireballData.explosionEffectPrefab != null)
            Instantiate(fireballData.explosionEffectPrefab, explosionPos, Quaternion.identity);

        // 폭발 범위 내의 모든 콜라이더 감지
        Collider[] colliders = Physics.OverlapSphere(explosionPos, fireballData.explosionRadius, fireballData.explosionLayer);

        Debug.Log($"[Fireball] 폭발 반경({fireballData.explosionRadius}) 내 감지된 물체 수: {colliders.Length}");

        foreach (Collider hit in colliders)
        {
            if (hit.gameObject.layer == fryingPanLayer)
            {
                if (photonView.IsMine)
                    hit.GetComponent<PanController>().OnTakeDamage(fireballData.damage);
            }
            // 파편(ChunkNode)인지 확인 (자신 또는 부모)
            ChunkNode node = hit.GetComponent<ChunkNode>();
            if (node == null) node = hit.GetComponentInParent<ChunkNode>();

            if (node != null)
            {
                // 감지된 파편에게 힘 적용
                node.ApplyExplosionForce(
                    fireballData.explosionForce,
                    explosionPos,
                    fireballData.explosionRadius,
                    fireballData.explosionUpward
                );
                continue;
            }

            // 일반 물리 객체(플레이어, 적 등) 처리
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            // 아군 오인사격 방지
            PhotonView targetView = hit.GetComponent<PhotonView>();
            if (targetView != null)
            {
                if (!targetView.IsMine) continue;
                if (targetView.OwnerActorNr == shooterActorNumber && hit.CompareTag("Player")) continue;
                if (hit.CompareTag("Player") && !IsFriendlyFireOn()) continue;
            }

            if (rb != null)
            {
                rb.AddExplosionForce(fireballData.explosionForce, explosionPos, fireballData.explosionRadius, fireballData.explosionUpward, ForceMode.Impulse);
            }

            // 데미지 처리
            if (targetView != null && targetView.IsMine)
            {
                IDamageable target = hit.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(fireballData.damage);
                }
            }
        }
    }

    private bool IsFriendlyFireOn()
    {
        if (PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("FriendlyFire", out object isFF))
        {
            return (bool)isFF;
        }
        return false;
    }

    public void SetShooterActorNumber(int actorNumber)
    {
        shooterActorNumber = actorNumber;
    }
}