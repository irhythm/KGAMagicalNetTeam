using Photon.Pun;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

public class MagicLightningStrike : MagicAction
{
    private LightningStrikeSO lightningData;

    public MagicLightningStrike(LightningStrikeSO data) : base(data)
    {
        this.lightningData = data;
    }

    public override void OnCast(Vector3 spawnPos, Vector3 targetPos, bool isLeftHand, int shooterID)
    {
        if (lightningData.itemPrefab != null)
        {
            GameObject obj = PhotonNetwork.Instantiate("EffectPrefab/" + lightningData.itemPrefab.name, targetPos, lightningData.itemPrefab.transform.rotation);

            LightningStrike strikeLogic = obj.GetComponent<LightningStrike>();
            if (strikeLogic != null)
            {
                strikeLogic.Setup(lightningData, shooterID);
            }
        }
    }
}