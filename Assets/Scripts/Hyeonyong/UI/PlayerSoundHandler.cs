using NUnit.Framework;
using Photon.Pun;
using Photon.Voice.Unity.UtilityScripts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class PlayerSoundHandler : MonoBehaviour
{
    PhotonView pv;
    MicAmplifier mic;
    AudioSource audio;
    static List<AudioSource> otherPlayerAudio = new List<AudioSource>();
    private void Start()
    {

        pv = GetComponent<PhotonView>();
        audio = GetComponent<AudioSource>();


        if (pv.IsMine)
        {
            Debug.Log("내거 값 가져옴");
            mic = GetComponent<MicAmplifier>();

            SetSoundEvent();
            SetMicSound(-1);
            SetVoiceSound(-1);
        }
        else
        {
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
}
