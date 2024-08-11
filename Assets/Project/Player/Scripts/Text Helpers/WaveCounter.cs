using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI displayText;
    [SerializeField] string startWaveText = "WAVE: [N]";
    [SerializeField] string waveNumberChar = "[N]";
    // Start is called before the first frame update
    void Start()
    {
        if (displayText == null)
            displayText = GetComponent<TextMeshProUGUI>();
        EnemyManager.OnRoundEnded.AddListener(_OnWaveEnd);
        _SetWave(1);
    }

    int _wave = 1;
    void _OnWaveEnd()
    {
        _wave++;
        _SetWave(_wave);
    }
    void _SetWave(int i)
    {
        string text = startWaveText.Replace(waveNumberChar, i.ToString());
        displayText.text = text;
    }
}
