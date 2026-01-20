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
    List<AudioSource> otherPlayerAudio = new List<AudioSource>();
    private void Start()
    {
        pv = GetComponent<PhotonView>();
        mic = GetComponent<MicAmplifier>();

        SetSoundEvent();
        SetMicSound(-1);
        SetVoiceSound(-1);
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
        if (value == -1)
        {
            mic.AmplificationFactor = UIManager.Instance.MicSound.value;
        }
        mic.AmplificationFactor = value;
    }
    private void SetVoiceSound(float value)
    {
        if (value == -1)
        {
            foreach (AudioSource source in otherPlayerAudio)
            {
                source.volume = UIManager.Instance.VoiceChatSound.value;
            }
        }
        foreach (AudioSource source in otherPlayerAudio)
        {
            source.volume = value;
        }
    }
}
