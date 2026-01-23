using Photon.Pun;
using UnityEngine;

public class PurchasableScriptableObject : MonoBehaviourPunCallbacks
{
    [SerializeField] InventoryDataSO _inventoryData;
    [SerializeField] int _cost;
    public InventoryDataSO InventoryData => _inventoryData;

    public int Cost => _cost;

    public void RequestDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
        else
        {
            //PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("RPC_Destroy", RpcTarget.MasterClient);
        }

    }
    [PunRPC]
    public void RPC_Destroy()
    {
        PhotonNetwork.Destroy(this.gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
