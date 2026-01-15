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
            if (colorIndex < 0 || colorIndex >= playerColors.Count)
            {
                colorIndex = 0; // 기본 색상 인덱스로 설정
            }
            photonView.RPC("ChangeColor", RpcTarget.AllBuffered, colorIndex);
        }
        else
        {
            photonView.RPC("ChangeColor", RpcTarget.AllBuffered, 0);
        }
    }

    [PunRPC]
    public void ChangeColor(int colorIndex)
    {
       
        transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material.color = playerColors[colorIndex];
        transform.GetChild(0).GetChild(1).GetComponent<Renderer>().material.color = playerColors[colorIndex];

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
