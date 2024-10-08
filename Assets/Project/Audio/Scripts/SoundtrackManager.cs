using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundtrackManager : MonoBehaviour
{
    public static SoundtrackManager instance;

    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip firstTrack;
    [SerializeField] private List<ContextualSoundtrack> _soundtracks;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        if (firstTrack != null)
        {
            _source.clip = firstTrack;
            _source.Play();
        }

    }

    private void Start()
    {
        EnemyManager.OnRoundEnded.AddListener(PlayBetweenRounds);
        EnemyManager.OnRoundStarted.AddListener(PlayCombat);
        GameStateManager.onGameLose += PlayMenu;
    }

    public static void PlayMenu()
    {
        _PlayContext("menu");
    }

    public static void PlayCombat()
    {
        _PlayContext("combat");
    }

    public static void PlayBetweenRounds()
    {
        _PlayContext("betweenrounds");
    }

    public static void Stop()
    {
        instance._source.Stop();
    }

    static void _PlayContext(string context)
    {
        if (instance == null) return;
        var c = _GetContext(context);
        _PlayContext(c);
    }
    static void _PlayContext(ContextualSoundtrack soundtrack)
    {
        instance._source.clip = soundtrack.clips.GetRandom();
        instance._source.Play();
    }

    static ContextualSoundtrack _GetContext(string context)
    {
        return instance._soundtracks.Find(x => x.context == context);
    }
}

[Serializable]
public struct ContextualSoundtrack
{
    public string context;
    public List<AudioClip> clips;
}
