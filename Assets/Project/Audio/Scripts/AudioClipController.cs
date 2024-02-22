using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioClipController : MonoBehaviour
{
    public new string name;
    [SerializeField] private List<AudioClip> _clips;
    private AudioSource _audioSource;
    [Range(0f, 1f)]
    [SerializeField]private float _maxInclusivePitchVariance;
    [Range(0f, 1f)]
    [SerializeField]private float _maxVolumeVariance;

    private float initialPitch;
    float initialVolume;

    public bool playOnAwake = false;
    public bool playOnEnable = false;
    public bool loop = false;

    private void Awake()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            return;

        if (_audioSource.outputAudioMixerGroup == null)
        {
            Debug.LogError($"MISSING AUDIO CHANNEL: {gameObject.FullPath()}", gameObject);
        }
        

        _audioSource.loop = loop;
        initialPitch = _audioSource.pitch;
        initialVolume = _audioSource.volume;
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

    public void PlayClipAt(Vector3 pos)
    {
        AudioClip clip = GetClip();
        AudioPool.PlaySoundAt(clip, pos);
    }
    public void PlayClip()
    {
        if (_clips.Count == 0) return;
        var clip = _clips.GetRandom();

        _audioSource.clip = clip;
        _audioSource.pitch = initialPitch + Random.Range(-_maxInclusivePitchVariance, _maxInclusivePitchVariance);
        _audioSource.volume = initialVolume + Random.Range(-_maxVolumeVariance, _maxVolumeVariance);
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
