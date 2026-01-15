using UnityEngine;

[CreateAssetMenu(fileName = "New Fireball", menuName = "Game/Fireball")]
public class Fireball : MagicData
{
    public float speed = 20f;

    public override void OnCast(Vector3 spawnPos, Vector3 direction, bool isLeftHand)
    {
        Vector3 finalSpawnPos = spawnPos + spawnOffset;

        GameObject obj = Instantiate(itemPrefab, finalSpawnPos, Quaternion.LookRotation(direction));

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        Debug.Log($"파이어볼 (손: {(isLeftHand ? "왼손" : "오른손")})");
    }
}
