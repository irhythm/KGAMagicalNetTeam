using UnityEngine;

public abstract class MagicDataSO : ActionItemDataSO
{
    [Header("Magic Setting")]
    public int damage;
    public float knockbackForce;
    public float radius = 5f;
    public float forceUpward = 1f;
    public Vector3 spawnOffset;
    public AudioClip magicSound;


    public abstract override ActionBase CreateInstance();
}