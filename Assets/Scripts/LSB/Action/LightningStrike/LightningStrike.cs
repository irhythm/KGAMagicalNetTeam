using Photon.Pun;
using System.Collections;
using UnityEngine;

public class LightningStrike : MonoBehaviourPun
{
    // [중요 변경] 인스펙터에서 할당할 수 있게 SerializeField 추가
    // 유니티 에디터의 프리팹에서 'LightningStrikeSO' 파일을 여기에 꼭 넣어주세요!
    [SerializeField] private LightningStrikeSO data;

    private int shooterID;

    public void Setup(LightningStrikeSO data, int shooterID)
    {
        if (this.data == null) this.data = data;

        this.shooterID = shooterID;

        Debug.Log("생성됨");

        StartCoroutine(StrikeRoutine());
        SoundManager.Instance.PlaySFX(data.magicSound, 1f, 100f, gameObject.transform.position);
    }

    private IEnumerator StrikeRoutine()
    {
        yield return CoroutineManager.waitForSeconds(data.strikeDelay);

        photonView.RPC(nameof(RPC_Strike), RpcTarget.All, shooterID);
    }

    [PunRPC]
    private void RPC_Strike(int _shooterID)
    {
        if (data == null) return;

        if (data.strikeEffectPrefab != null)
        {
            Instantiate(data.strikeEffectPrefab, transform.position, Quaternion.identity);
        }

        SoundManager.Instance.PlaySFX(data.lightningSound, 1f, 100f, gameObject.transform.position);

        Collider[] colliders = Physics.OverlapSphere(transform.position, data.strikeRadius, data.hitLayer);

        foreach (var col in colliders)
        {
            PhotonView targetView = col.GetComponent<PhotonView>();

            // [변경] 받아온 파라미터(_shooterID)와 비교
            if (targetView != null && targetView.OwnerActorNr == _shooterID)
            {
                if (col.CompareTag("Player"))
                    continue;
            }

            IExplosion targetComponent = col.GetComponent<IExplosion>();
            if (targetComponent == null) targetComponent = col.GetComponentInParent<IExplosion>();

            if (targetComponent != null)
            {
                targetComponent.OnExplosion(transform.position, data, _shooterID);
            }
            else
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(data.knockbackForce, transform.position, data.strikeRadius, data.forceUpward, ForceMode.Impulse);
                }
            }

            if (photonView.IsMine)
            {
                IDamageable target = col.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(data.damage);
                }
            }
        }

        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}