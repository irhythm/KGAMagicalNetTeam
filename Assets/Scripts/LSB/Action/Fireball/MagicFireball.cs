using Photon.Pun;
using UnityEngine;

public class MagicFireball : MagicAction
{
    private FireballSO fireballData;

    public MagicFireball(FireballSO data) : base(data)
    {
        this.fireballData = data;
    }

    public override void OnCast(Vector3 spawnPos, Vector3 targetPos, bool isLeftHand, int shooterID)
    {
        Vector3 finalSpawnPos = spawnPos + fireballData.spawnOffset;

        Vector3 direction = (targetPos - finalSpawnPos).normalized;

        if (fireballData.itemPrefab != null)
        {
            GameObject obj = PhotonNetwork.Instantiate("EffectPrefab/" + fireballData.itemPrefab.name, finalSpawnPos, Quaternion.LookRotation(direction));

            Fireball fireball = obj.GetComponent<Fireball>();
            if (fireball != null)
            {
                fireball.SetShooterActorNumber(shooterID);
            }
        }
    }
}