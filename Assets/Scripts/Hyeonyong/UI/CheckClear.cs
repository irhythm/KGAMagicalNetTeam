using Photon.Pun;
using UnityEngine;

public class CheckClear : MonoBehaviour
{
    int curPlayerEnter = 0;
    private void OnTriggerEnter(Collider other)
    {
        //if (!PhotonNetwork.IsMasterClient)
        //    return;


        if (other.CompareTag("Player"))
        {
            curPlayerEnter++;
            GameManager.Instance.CheckRoundClear(curPlayerEnter);
        }
        else if (other.CompareTag("Money"))
        {
            GameManager.Instance.PlusMoneyCount();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //if (!PhotonNetwork.IsMasterClient)
        //    return;
        if (!other.CompareTag("Player"))
            return;
        Debug.Log("플레이어가 나갔다");
        curPlayerEnter--;
        //GameManager.Instance.CheckRoundClear(curPlayerEnter);
    }
}
