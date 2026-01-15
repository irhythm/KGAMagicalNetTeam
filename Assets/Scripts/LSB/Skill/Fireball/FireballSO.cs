using UnityEngine;

[CreateAssetMenu(fileName = "New Fireball", menuName = "Game/Fireball")]
public class FireballSO : MagicDataSO
{
    public float speed = 20f;

    public override MagicBase CreateInstance()
    {
        return new Fireball(this);
    }
}
