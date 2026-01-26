using UnityEngine;

public interface IChainLightningTarget
{
    void OnChainHit(Vector3 hitPos, LightningStrikeSO data, int shooterID);
}
