using UnityEngine;

public class MagicFireball : MagicBase
{
    private FireballSO fireballData;

    public MagicFireball(FireballSO data) : base(data)
    {
        this.fireballData = data;
    }

    public override void OnCast(Vector3 spawnPos, Vector3 direction, bool isLeftHand, int shooterID)
    {
        Vector3 finalSpawnPos = spawnPos + fireballData.spawnOffset;

        if (fireballData.itemPrefab != null)
        {
            GameObject obj = GameObject.Instantiate(fireballData.itemPrefab, finalSpawnPos, Quaternion.LookRotation(direction));
            Fireball fireball = obj.GetComponent<Fireball>();
            fireball.Init(fireballData);
            fireball.SetShooterActorNumber(shooterID);
        }
    }
}
