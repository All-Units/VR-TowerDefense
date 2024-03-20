using UnityEngine;

public class AudioSourcePool : PollingPool<AudioSource>
{
    public AudioSourcePool(AudioSource prefab) : base(prefab){}

    protected override bool IsActive(AudioSource component)
    {
        return component.time <= component.clip.length;
    }

    public void Play(Vector3 pos)
    {
        var obj = _Get();
        obj.gameObject.SetActive(true);
        obj.transform.position = pos;
    }
}