using Photon.Pun;
using UnityEngine;

public class Fireball : MonoBehaviourPun
{
    [SerializeField] private FireballSO fireballData;

    private int shooterActorNumber;
    private bool hasExploded = false;

    private void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * fireballData.speed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 쏜 사람만 충돌 감지 수행
        if (!photonView.IsMine) return;

        // 나 자신은 무시
        PhotonView targetView = other.GetComponent<PhotonView>();
        if (targetView != null && targetView.OwnerActorNr == shooterActorNumber)
        {
            if(other.CompareTag("Player"))
                return;
        }

        if (hasExploded) return;
        hasExploded = true;

        photonView.RPC(nameof(RPC_ExplodeProcess), RpcTarget.All, transform.position);

        // 투사체 제거
        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    private void RPC_ExplodeProcess(Vector3 explosionPos)
    {
        if (fireballData.explosionEffectPrefab != null)
        {
            Instantiate(fireballData.explosionEffectPrefab, explosionPos, Quaternion.identity);
        }

        Collider[] colliders = Physics.OverlapSphere(explosionPos, fireballData.explosionRadius, fireballData.explosionLayer);
        Debug.Log($"Explosion hit {colliders.Length} colliders.");

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            PhotonView targetView = hit.GetComponent<PhotonView>();
            ChunkNode chunkNode = hit.GetComponent<ChunkNode>(); // [추가] 청크 노드 가져오기

            if (chunkNode != null)
            {
                float distance = Vector3.Distance(explosionPos, hit.ClosestPoint(explosionPos));
                float impactFactor = 1f - Mathf.Clamp01(distance / fireballData.explosionRadius);
                float effectiveForce = fireballData.explosionForce * impactFactor;

                chunkNode.ApplyExplosionForce(
                    effectiveForce,
                    fireballData.explosionForce,
                    explosionPos,
                    fireballData.explosionRadius,
                    fireballData.explosionUpward
                );
            }
            if (targetView != null)
            {
                if (!targetView.IsMine) continue;

                if (targetView.OwnerActorNr == shooterActorNumber)
                {
                    if (hit.CompareTag("Player"))
                    {
                        continue;
                    }
                }

                if (hit.CompareTag("Player") && !IsFriendlyFireOn()) continue;
            }

            if (rb != null && !rb.isKinematic)
            {
                rb.AddExplosionForce(fireballData.explosionForce, explosionPos, fireballData.explosionRadius, fireballData.explosionUpward, ForceMode.Impulse);
            }

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