using Photon.Pun;
using UnityEngine;

public class Fireball : MonoBehaviourPun
{
    [Header("Settings")]
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
        if (!photonView.IsMine) return;

        PhotonView targetView = other.GetComponent<PhotonView>();
        if (targetView != null && targetView.OwnerActorNr == shooterActorNumber)
        {
            return;
        }

        if (hasExploded) return;
        hasExploded = true;

        photonView.RPC(nameof(RPC_ExplodeProcess), RpcTarget.All, transform.position);

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

        foreach (Collider hit in colliders)
        {
            DestructibleWall wall = hit.GetComponentInParent<DestructibleWall>();
            if (wall != null)
            {
                wall.OnHit(explosionPos, fireballData.explosionForce, fireballData.explosionRadius, fireballData.explosionUpward);

                continue;
            }

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            PhotonView targetView = hit.GetComponent<PhotonView>();

            if (targetView != null)
            {
                if (!targetView.IsMine) continue;

                if (targetView.OwnerActorNr == shooterActorNumber && hit.CompareTag("Player")) continue;

                if (hit.CompareTag("Player") && !IsFriendlyFireOn()) continue;
            }

            if (rb != null)
            {
                var agent = hit.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null && agent.enabled) agent.enabled = false;

                rb.isKinematic = false;
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

    private void OnDrawGizmosSelected()
    {
        if (fireballData != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, fireballData.explosionRadius);
        }
    }
}