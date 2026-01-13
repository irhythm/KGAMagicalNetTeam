using UnityEngine;

public sealed class SoundPool : ObjectPoolManager<AudioSource>
{
    protected override void DisablePoolObject(AudioSource obj)
    {
        base.DisablePoolObject(obj);
        obj.clip = null;
    }
}