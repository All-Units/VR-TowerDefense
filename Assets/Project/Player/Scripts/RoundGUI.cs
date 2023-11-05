using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class RoundGUI : MonoBehaviour
{
    [SerializeField] private GameObject roundStartPanel;
    [SerializeField] private GameObject roundEndPanel;
    [SerializeField] private TextMeshProUGUI roundStartText;
    [SerializeField] private TextMeshProUGUI roundEndText;
    [SerializeField] private float displayTime = 3f;
    private string _roundStartString;
    // Start is called before the first frame update
    void Start()
    {
        roundStartPanel.SetActive(false);
        roundEndPanel.SetActive(false);
        _roundStartString = roundStartText.text;
        roundEndText.text = roundEndText.text.Replace("[TIME]", EnemySpawner.WaveDelay.ToString());
        EnemySpawner.OnRoundStarted.AddListener(_OnRoundStart);
        EnemySpawner.OnRoundEnded.AddListener(_OnRoundEnd);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void _OnRoundStart()
    {
        roundStartText.text = _roundStartString.Replace("[N]", EnemySpawner.CurrentWave.ToString());
        StartCoroutine(fadeGOAfter(roundStartPanel));
    }

    void _OnRoundEnd()
    {
        StartCoroutine(fadeGOAfter(roundEndPanel));
    }

    IEnumerator fadeGOAfter(GameObject go)
    {
        go.SetActive(true);
        yield return new WaitForSeconds(displayTime);
        go.SetActive(false);
    }
}
