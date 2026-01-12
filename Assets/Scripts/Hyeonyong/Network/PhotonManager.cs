using NUnit.Framework;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PhotonManager : PhotonSingleton<PhotonManager>, IPunObservable
{

    public Hashtable playerTable = new Hashtable();
    //나중에 
    public List<float> sendObservableFloat = new List<float>();

    //RPC 쏘기
    public void SendRPCAll(PhotonView pv, string name, RpcTarget target, params object[] args)
    {
        if ((args == null || args.Length == 0))
        {
            pv.RPC(name, target);
        }
        else
        {
            pv.RPC(name,target,args);
        }
    }
    //호스트 전용 RPC
    public void SendRPCAllOnlyHost(PhotonView pv, string name, RpcTarget target, params object[] args)
    {
        if(!PhotonNetwork.IsMasterClient)
            return;
        if ((args == null || args.Length == 0))
        {
            pv.RPC(name, target);
        }
        else
        {
            pv.RPC(name,target,args);
        }
    }

    public void SetProperty(string key, string value)
    {
        playerTable[key] = value;

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerTable);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //프로퍼티 변경 시 실행하고자 하는 코드
    }


    //처음 등록시 값 저장 및 인덱스 반환
    public int SetObservableIndex(float num)
    {
        sendObservableFloat.Add(num);
        return sendObservableFloat.Count-1;
    }
    //이후 인덱스를 통해 값 반환
    public float ReturnObservableData(int index)
    {
        return sendObservableFloat[index];
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            foreach (float f in sendObservableFloat)
            {
                stream.SendNext(f);
            }

        }
        else
        {
            for (int i = 0; i < sendObservableFloat.Count; i++)
            {
                sendObservableFloat[i] = (float)stream.ReceiveNext();
            }
        }
    }
}
