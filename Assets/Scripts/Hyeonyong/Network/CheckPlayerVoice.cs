using Photon.Pun;
using Photon.Voice.PUN;
using UnityEngine;
using UnityEngine.UI;

public class CheckPlayerVoice : MonoBehaviour
{
    [SerializeField] Image speakerImage;
    PhotonVoiceView pvv;
    PhotonView pv;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pv= GetComponent<PhotonView>();
        pvv=GetComponent<PhotonVoiceView>();
        speakerImage.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (pv.IsMine)
        {
            if(pvv.IsRecording)
                Debug.Log("말하는 중");
            speakerImage.enabled = pvv.IsRecording;
        }
        else
        {
            if (pvv.IsSpeaking)
            {
                Debug.Log("듣는 중");
            }
            speakerImage.enabled= pvv.IsSpeaking;
        }
    }

}
