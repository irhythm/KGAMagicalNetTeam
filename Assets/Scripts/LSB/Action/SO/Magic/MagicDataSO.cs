using UnityEngine;

public abstract class MagicDataSO : ActionItemDataSO
{
    [Header("Magic Setting")]
    public int damage;
    public Vector3 spawnOffset;
    public float knockbackForce;

    public abstract override ActionBase CreateInstance();
}