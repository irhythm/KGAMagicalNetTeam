using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class ColorChangeScript : MonoBehaviourPunCallbacks
{
    //260113 최정욱 WizardColor 시스템 적용
    [SerializeField] List<Color> playerColors = new List<Color>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GetComponent<PhotonView>().IsMine == false)
        {
            return;
        }
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("WizardColor"))
        {


            int colorIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["WizardColor"] - 1;
            photonView.RPC("ChangeColor", RpcTarget.AllBuffered, colorIndex);
        }
    }

    [PunRPC]
    public void ChangeColor(int colorIndex)
    {
        if (colorIndex == 0)
        {
            return;
        }
        transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material.color = playerColors[colorIndex];
        transform.GetChild(0).GetChild(1).GetComponent<Renderer>().material.color = playerColors[colorIndex];

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
