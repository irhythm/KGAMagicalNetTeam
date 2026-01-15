using UnityEngine;

public class Fireball : MagicBase
{
    private FireballSO fireballData;

    public Fireball(FireballSO data) : base(data)
    {
        this.fireballData = data;
    }

    public override void OnCast(Vector3 spawnPos, Vector3 direction, bool isLeftHand)
    {
        Vector3 finalSpawnPos = spawnPos + fireballData.spawnOffset;

        if (fireballData.itemPrefab != null)
        {
            GameObject obj = GameObject.Instantiate(fireballData.itemPrefab, finalSpawnPos, Quaternion.LookRotation(direction));

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * fireballData.speed;
            }
        }

        Debug.Log($"파이어볼 발사! (데미지: {fireballData.magicDamage}, 손: {(isLeftHand ? "왼손" : "오른손")})");
    }
}
