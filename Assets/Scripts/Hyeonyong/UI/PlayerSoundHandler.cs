using Photon.Pun;
using Photon.Voice.Unity.UtilityScripts;
using Photon.Voice.Unity;
using System.Collections.Generic;
using UnityEngine;
using Photon.Voice.PUN;

public class PlayerSoundHandler : MonoBehaviour
{
    PhotonView pv;
    MicAmplifier mic;
    AudioSource audio;
    Recorder recorder;
    static List<AudioSource> otherPlayerAudio = new List<AudioSource>();

    private void Start()
    {

        pv = GetComponent<PhotonView>();
        audio = GetComponent<AudioSource>();
        mic = GetComponent<MicAmplifier>();
        recorder = GetComponent<Recorder>();

        if (pv.IsMine)
        {
            Debug.Log("내거 값 가져옴");
            SetSoundEvent();
            //SetMicSound(-1);
            //SetVoiceSound(-1);
            PunVoiceClient.Instance.PrimaryRecorder = recorder;
        }
        else
        {
            recorder.enabled = false;
            mic.enabled = false;
            otherPlayerAudio.Add(audio);
        }
    }
    private void OnDisable()
    {
        otherPlayerAudio.Remove(audio);
    }
    private void SetSoundEvent()
    {
        UIManager.Instance.MicSound.onValueChanged.AddListener((value) =>
            {
                SetMicSound(value);
            });
        UIManager.Instance.VoiceChatSound.onValueChanged.AddListener((value) =>
        {
            SetVoiceSound(value);
        });
        UIManager.Instance.VoiceChatSoundMute.onValueChanged.AddListener(isOn =>
        {
            SetVoiceMute(isOn);
        });
        UIManager.Instance.MicSoundMute.onValueChanged.AddListener(isOn =>
        {
            SetMicMute(isOn);
        });

        SetMicSound(UIManager.Instance.MicSound.value);
        SetVoiceSound(UIManager.Instance.VoiceChatSound.value);
        SetVoiceMute(UIManager.Instance.VoiceChatSoundMute.isOn);
        SetMicMute(UIManager.Instance.MicSoundMute.isOn);
        //UIManager.Instance.VoiceChatSoundMute.isOn = PlayerPrefsDataManager.PlayerVoiceMute;
        ////처음에 값을 가져올 때 
        //UIManager.Instance.MicSoundMute.isOn = PlayerPrefsDataManager.PlayerMicMute;

    }

    private void SetMicSound(float value)
    {
        Debug.Log("내거 값 가져옴 새탕 "+value);
        //이거 시작하자마자 받아와서 처음 값으로 진행되는거 같다 그것도 아닌데?
        if (value == -1)
        {
            Debug.Log("UI 매니저에서 값 가져옴 : " + UIManager.Instance.MicSound.value);
            mic.AmplificationFactor = UIManager.Instance.MicSound.value;
        }
        else
        {
            mic.AmplificationFactor = value;
        }
    }
    private void SetVoiceSound(float value)
    {
        if (value == -1)
        {
            foreach (AudioSource source in otherPlayerAudio)
            {
                //if(source.GetComponent<PhotonView>().IsMine)
                //     continue;
                source.volume = UIManager.Instance.VoiceChatSound.value;
            }
        }
        else
        {
            foreach (AudioSource source in otherPlayerAudio)
            {
                //    if (source.GetComponent<PhotonView>().IsMine)
                //        continue;
                source.volume = value;
            }
        }
    }

    private void SetVoiceMute(bool check)
    {
        foreach (AudioSource source in otherPlayerAudio)
        {
            source.mute = check;
        }
    }
    private void SetMicMute(bool check)
    {
        if (check)
        {
            mic.AmplificationFactor = 0;
        }
        else
        {
            mic.AmplificationFactor = UIManager.Instance.MicSound.value;
        }
    }
}
