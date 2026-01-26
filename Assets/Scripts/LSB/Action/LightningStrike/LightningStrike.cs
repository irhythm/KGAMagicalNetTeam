using Photon.Pun;
using System.Collections;
using UnityEngine;

public class LightningStrike : MonoBehaviourPun
{
    private LightningStrikeSO data;
    private int shooterID;

    public void Setup(LightningStrikeSO data, int shooterID)
    {
        this.data = data;
        this.shooterID = shooterID;

        Debug.Log("생성됨");

        StartCoroutine(StrikeRoutine());
        SoundManager.Instance.PlaySFX(data.magicSound, 1f, 100f, gameObject.transform.position);
    }

    private IEnumerator StrikeRoutine()
    {
        yield return CoroutineManager.waitForSeconds(data.strikeDelay);

        photonView.RPC(nameof(RPC_Strike), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_Strike()
    {
        if (data.strikeEffectPrefab != null)
        {
            Instantiate(data.strikeEffectPrefab, transform.position, Quaternion.identity);
        }

        if (photonView.IsMine)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, data.strikeRadius, data.hitLayer);

            Debug.Log($"충돌한 오브젝트 수: {colliders.Length}");

            foreach (var col in colliders)
            {
                PhotonView targetView = col.GetComponent<PhotonView>();
                if (targetView != null && targetView.OwnerActorNr == shooterID)
                {
                    if (col.CompareTag("Player"))
                        continue;
                }

                ChunkNode node = col.GetComponent<ChunkNode>();
                if (node == null) node = col.GetComponentInParent<ChunkNode>();

                if (node != null)
                {
                    node.ApplyExplosionForce(data.knockbackForce, transform.position, data.strikeRadius, 0.5f);
                }

                IDamageable target = col.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(data.damage);
                }
            }
            SoundManager.Instance.PlaySFX(data.lightningSound, 1f, 100f, gameObject.transform.position);
            PhotonNetwork.Destroy(gameObject);
        }
    }
}