using UnityEngine;

public interface IExplosion
{
    void OnExplosion(Vector3 explosionPos, FireballSO data, int attackerActorNr);
}
