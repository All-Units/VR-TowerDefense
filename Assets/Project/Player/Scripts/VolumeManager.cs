using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    
    private static AudioMixer mixer;
    public bool CheckForInvalids = false;
    public float CheckAudioSourceRate = 1f;
    private void Awake()
    {
        if (mixer == null)
            mixer = Resources.Load<AudioMixer>("MainMixer");
#if UNITY_EDITOR
        if (CheckForInvalids)
            StartCoroutine(_CheckForValidAudioSources());
#endif
        string[] sliderTypes = new[] { "master", "soundtrack", "sfx" };
        foreach (string type in sliderTypes)
        {
            if (PlayerPrefs.HasKey(type))
                _SetVolume(type, PlayerPrefs.GetFloat(type));
            else
            {
                print($"Player prefs missing :{type}");
            }

        }

    }
    public static void InitValuesFromCache()
    {
        string[] sliderTypes = new[] { "master", "soundtrack", "sfx" };
        foreach (string type in sliderTypes)
        {
            if (PlayerPrefs.HasKey(type))
                _SetVolume(type, PlayerPrefs.GetFloat(type));
        }
    }
    string[] whitelist = new string[] { "TowerDeathSounds", "PlaceTowerSound", "RoundStart(Clone)",
        "SoundManager", "RoundEnded(Clone)" };
    IEnumerator _CheckForValidAudioSources()
    {
        var whitelist = this.whitelist.ToHashSet();
        while (true) { 
            yield return new WaitForSeconds(CheckAudioSourceRate);

            var sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (var source in sources)
            {
                if (source.outputAudioMixerGroup == null)
                {
                    Debug.LogError($"MISSING AUDIO CHANNEL: {source.gameObject.FullPath()}", source.gameObject);
                }
                if (source.spatialBlend < 0.9f && whitelist.Contains(source.gameObject.name.Trim()) == false)
                    Debug.Log($"{source.gameObject.name} HAD 2D SOUND", source.gameObject);
            }
        }

    }
    
    public void SetMasterVolume(System.Single val)
    {
        float volume = (float)val;
        _SetVolume("master", volume);
    }
    public void SetSoundtrackVolume(System.Single val)
    {
        float volume = (float)val;
        _SetVolume("soundtrack", volume);
    }
    public void SetSFXtrackVolume(System.Single val)
    {
        float volume = (float)val;
        _SetVolume("sfx", volume);
    }
    public void ResetAll()
    {
        foreach (Slider slider in GetComponentsInChildren<Slider>())
        {
            slider.value = slider.maxValue;
            slider.onValueChanged.Invoke(slider.value);
        }
    }


    Dictionary<TextMeshProUGUI, Slider> _sliderValueLabels = new Dictionary<TextMeshProUGUI, Slider>();
    /// <summary>
    /// Formats the volume to human readable percents
    /// </summary>
    /// <param name="text"></param>
    public void OnVolumeChanged(TextMeshProUGUI text)
    {
        Slider slider;
        if (_sliderValueLabels.ContainsKey(text) == false)
        {
            slider = text.GetComponentInParent<Slider>();
            _sliderValueLabels[text] = slider;
        }
        else
            slider = _sliderValueLabels[text];
        float val = slider.value;
        int percent = (int)(val * 100f);
        text.text = $"{percent}%";



    }
    static void _SetVolume(string track, float volume)
    {
        if (mixer == null)
            mixer = Resources.Load<AudioMixer>("MainMixer");
        PlayerPrefs.SetFloat(track, volume);
        volume = Mathf.Log10(volume) * 20;
        bool b = mixer.SetFloat(track, volume);
        if (b == false)
        {
            Debug.LogError($"{track} was not a valid volume type");
        }
        
    }
}
