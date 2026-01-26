using UnityEngine;

public interface IExplosion
{
    void OnExplosion(Vector3 explosionPos, MagicDataSO data, int attackerActorNr);
}
