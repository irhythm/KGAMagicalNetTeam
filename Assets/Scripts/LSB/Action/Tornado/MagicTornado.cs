using Photon.Pun;
using UnityEngine;

public class MagicTornado : MagicAction
{
    private TornadoSO tornadoData;

    public MagicTornado(TornadoSO data) : base(data)
    {
        this.tornadoData = data;
    }

    public override void OnCast(Vector3 spawnPos, Vector3 targetPos, bool isLeftHand, int shooterID)
    {
        if (tornadoData.itemPrefab != null)
        {
            Vector3 finalSpawnPos = spawnPos + tornadoData.spawnOffset;

            Vector3 direction = (targetPos - finalSpawnPos).normalized;
            direction.y = 0;

            if (direction == Vector3.zero) direction = Vector3.forward;

            Quaternion rotation = Quaternion.LookRotation(direction);

            GameObject obj = PhotonNetwork.Instantiate("EffectPrefab/" + tornadoData.itemPrefab.name, finalSpawnPos, rotation);

            PhotonView pv = obj.GetComponent<PhotonView>();
            if (pv != null)
            {
                pv.RPC("RPC_Setup", RpcTarget.AllBuffered, shooterID);
            }
        }
    }
}