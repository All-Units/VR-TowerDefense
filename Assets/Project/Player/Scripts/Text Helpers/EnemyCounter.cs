using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI displayText;
    [SerializeField] string startWaveText = "ENEMIES ALIVE: [N]";
    [SerializeField] string numberChar = "[N]";
    

    // Start is called before the first frame update
    void Start()
    {
        if (displayText == null)
            displayText = GetComponent<TextMeshProUGUI>();
        EnemyManager.instance.OnEnemySpawned.AddListener(_OnEnemyChange);
        EnemyManager.instance.OnEnemyKilled.AddListener(_OnEnemyChange);
        _SetWave(0);
    }

    int _wave = 1;
    void _OnEnemyChange()
    {
        _wave = EnemyManager.CurrentEnemyCount;
        _SetWave(_wave);
    }
    void _SetWave(int i)
    {
        string text = startWaveText.Replace(numberChar, i.ToString());
        displayText.text = text;
    }
}
