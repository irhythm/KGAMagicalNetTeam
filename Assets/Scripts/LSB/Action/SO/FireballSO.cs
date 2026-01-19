using UnityEngine;

[CreateAssetMenu(fileName = "New Fireball", menuName = "Game/Fireball")]
public class FireballSO : MagicDataSO
{
    public float speed = 20f;
    public GameObject explosionEffectPrefab;

    public override ActionBase CreateInstance()
    {
        return new MagicFireball(this);
    }
}