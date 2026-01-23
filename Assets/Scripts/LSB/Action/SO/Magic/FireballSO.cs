using UnityEngine;

[CreateAssetMenu(fileName = "New Fireball", menuName = "Game/Fireball")]
public class FireballSO : MagicDataSO
{
    public float speed = 20f;
    public GameObject explosionEffectPrefab;

    [Header("Explosion")]
    public float explosionRadius = 5f;      // 폭발 반경
    public float explosionUpward = 1f;      // 위로 띄우는 힘
    public LayerMask explosionLayer;        // 폭발에 맞을 레이어

    public override ActionBase CreateInstance()
    {
        return new MagicFireball(this);
    }
}