using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundCounterDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayText;
    // Start is called before the first frame update
    void Start()
    {
        UpdateWaveText();
        EnemyManager.OnRoundStarted.AddListener(UpdateWaveText);
    }

    void UpdateWaveText()
    {
        displayText.text = $"{EnemyManager.CurrentWave} / {EnemyManager.MaxWaves}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
