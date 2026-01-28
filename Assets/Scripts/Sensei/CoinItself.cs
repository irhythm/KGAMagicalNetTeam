using UnityEngine;
using Photon.Pun;

public class CoinItself : MonoBehaviourPunCallbacks
{
    [SerializeField] CoinSO coinData;
    public InventoryDataSO CoinData => coinData;


    public void RequestDestroy()
    {
        SoundManager.Instance.PlaySFX(coinData.CoinPickupSFX, 1f, 100f, transform.position);

        if (PhotonNetwork.IsMasterClient || photonView.IsMine)
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
        if (this.gameObject != null)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }


    public void DropCoin(Vector3 spawnPos, Vector3 targetPos)
    {
        SoundManager.Instance.PlaySFX(coinData.CoinThrowSFX, 1f, 100f, transform.position);
        photonView.RPC("RPC_DropCoin", RpcTarget.All, spawnPos, targetPos);
        GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().Inventory.RemoveItem(coinData);
    }


    [PunRPC]
    private void RPC_DropCoin(Vector3 spawnPos, Vector3 targetPos)
    {
        CoinSO coinSOData = coinData as CoinSO;
        GetComponent<Rigidbody>().AddForce((targetPos - spawnPos) * (coinSOData.CoinThrowForce), ForceMode.Impulse);


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
