using UnityEngine;

public sealed class SoundPool : SinglePoolManager<AudioSource>
{
    protected override void DisablePoolObject(AudioSource obj)
    {
        base.DisablePoolObject(obj);
        obj.clip = null;
    }
}