using UnityEngine;
using Photon.Pun;

public class CoinItself : MonoBehaviourPunCallbacks
{
    [SerializeField] InventoryDataSO coinData;
    public InventoryDataSO CoinData => coinData;


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
