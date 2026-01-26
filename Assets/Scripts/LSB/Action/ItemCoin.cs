using UnityEngine;
using Photon.Pun;

public class ItemCoin : ItemAction
{
    private CoinSO coinData;

    public ItemCoin(ActionItemDataSO data) : base(data)
    {
        this.coinData = data as CoinSO;
    }


    GameObject temp;
    public override void OnUse(Vector3 spawnPos, Vector3 targetPos, bool isLeftHand, int shooterID)
    {
        
        
        //PhotonNetwork.Instantiate("CoinEffect", spawnPos, Quaternion.identity).GetComponent<PhotonView>()
            //.RPC(nameof(RPC_DropCoin), RpcTarget.All, spawnPos);
            //RpcTarget


        temp = PhotonNetwork.Instantiate(coinData.itemPrefab.name, spawnPos, Quaternion.identity);
        temp.GetComponent<PhotonView>().RPC(nameof(RPC_DropCoin), RpcTarget.All, spawnPos, targetPos);

    }


    [PunRPC]
    private void RPC_DropCoin(Vector3 spawnPos, Vector3 targetPos)
    {

        temp.GetComponent<Rigidbody>().AddForce((targetPos - spawnPos) * 5f, ForceMode.Impulse);
        
        
    }


}
