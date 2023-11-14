using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioPool : MonoBehaviour
{
    [SerializeField] private GameObject AudioSourcePrefab;

    [SerializeField] private int InitialPoolSize = 20;

    public static AudioPool instance;

    private HashSet<AudioSource> _availableSources = new HashSet<AudioSource>();
    private HashSet<AudioSource> _InUseSources = new HashSet<AudioSource>();
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < InitialPoolSize; i++)
        {
            _AddSource();
        }

        instance = this;
    }

    AudioSource _GetSource()
    {
        if (_availableSources.Count == 0)
        {
            _AddSource();
        }
        var source = _availableSources.First();
        return source;
    }
    void _AddSource()
    {
        GameObject source = Instantiate(AudioSourcePrefab, transform);
        _availableSources.Add(source.GetComponent<AudioSource>());
    }

    public static void PlaySoundAt(AudioClip clip, Vector3 pos)
    {
        if(instance == null)
        {
            Debug.LogError("No Audio Pool Instance Found!");
            return;
        }
        var source = instance._GetSource();
        var release = instance._releaseSourceAfter(source, clip.length);
        instance.StartCoroutine(release);
        source.clip = clip;
        source.Play();
        source.transform.position = pos;
    }

    IEnumerator _releaseSourceAfter(AudioSource source, float t)
    {
        _availableSources.Remove(source);
        yield return new WaitForSeconds(t);
        _availableSources.Add(source);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
