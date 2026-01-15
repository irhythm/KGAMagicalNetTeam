using Photon.Pun;
using UnityEngine;

public class Fireball : MonoBehaviourPun
{
    [SerializeField] private FireballSO fireballData;

    private int shooterActorNumber;

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

        IDamageable target = other.GetComponent<IDamageable>();

        if (target == null)
        {
            PlayExplosionEffect(transform.position);
            if (photonView.IsMine && gameObject != null)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            
            return;
        }

        bool applyDamage = true;

        if (other.CompareTag("Player"))
        {
            PhotonView targetPlayerView = other.GetComponent<PhotonView>();

            // 내가 쏜거면 무시
            if (targetPlayerView != null && targetPlayerView.OwnerActorNr == shooterActorNumber)
            {
                Debug.Log("내가 쏜거 내가 맞음");
                return;
            }

            if (!IsFriendlyFireOn())
            {
                applyDamage = false; // 오사 꺼져 있으면 데미지 없음
            }
        }

        // 오사 켜져있으면 데미지 적용
        if (applyDamage)
        {
            target.TakeDamage(fireballData.damage);
            Debug.Log("아군 오사 했음");
        }

        // 일단 맞으면 폭발 이펙트 재생하고 삭제함
        PlayExplosionEffect(transform.position);
        if (photonView.IsMine && gameObject != null)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    // 오사 설정 켜져있나 확인
    private bool IsFriendlyFireOn()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("FriendlyFire", out object isFF))
        {
            return (bool)isFF;
        }
        return false;
    }

    // 폭발 이펙트 재생용
    private void PlayExplosionEffect(Vector3 pos)
    {
        if (fireballData.explosionEffectPrefab != null)
        {
            PhotonNetwork.Instantiate("EffectPrefab/" + fireballData.explosionEffectPrefab.name, pos, Quaternion.identity);
        }
    }

    public void SetShooterActorNumber(int actorNumber)
    {
        shooterActorNumber = actorNumber;
    }
}
