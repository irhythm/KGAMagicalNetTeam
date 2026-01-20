using UnityEngine;

[CreateAssetMenu(fileName = "New Fireball", menuName = "Game/Fireball")]
public class FireballSO : MagicDataSO
{
    public float speed = 20f;
    public GameObject explosionEffectPrefab;

    [Header("Explosion")]
    public float explosionRadius = 5f;      // Æø¹ß ¹Ý°æ
    public float explosionForce = 1000f;    // Æø¹ß Èû
    public float explosionUpward = 1f;      // À§·Î ¶ç¿ì´Â Èû
    public LayerMask explosionLayer;        // Æø¹ß¿¡ ¸ÂÀ» ·¹ÀÌ¾î

    public override ActionBase CreateInstance()
    {
        return new MagicFireball(this);
    }
}