using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioClipController : MonoBehaviour
{
    public new string name;
    [SerializeField] private List<AudioClip> _clips;
    private AudioSource _audioSource;
    [SerializeField]private float _maxInclusivePitchVariance;
    private float initialPitch;

    public bool playOnAwake = false;
    public bool playOnEnable = false;
    public bool loop = false;

    private void Awake()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            return;
        _audioSource.loop = loop;
        initialPitch = _audioSource.pitch;
        if (playOnAwake)
        {
            PlayClip();
        }
    }

    private void OnEnable()
    {
        if (playOnEnable)
            PlayClip();
    }

    public AudioClip GetClip()
    {
        return _clips.GetRandom();
    }
    public void PlayClipAtPos()
    {
        var pos = transform.position;
        PlayClipAt(pos);
    }
    public void PlayClipAt(Vector3 pos)
    {
        AudioClip clip = GetClip();
        AudioPool.PlaySoundAt(clip, pos);
    }
    public void PlayClip()
    {
        var clip = _clips.GetRandom();

        _audioSource.clip = clip;
        _audioSource.pitch = initialPitch + Random.Range(-_maxInclusivePitchVariance, _maxInclusivePitchVariance);
        _audioSource.Play();
    }

    public void Stop()
    {
        _audioSource.Stop();
    }

    private void OnValidate()
    {
        _audioSource = GetComponent<AudioSource>();
    }
}
