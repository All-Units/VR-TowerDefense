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
    [SerializeField] private float distanceFromPlayer = 3f;
    [SerializeField] private float height = 2f;
    private string _roundStartString;
    private string _roundEndString;

    Dictionary<TextMeshProUGUI, string> startStringsEndPanel = new Dictionary<TextMeshProUGUI, string>();

    // Start is called before the first frame update
    void Start()
    {
        roundStartPanel.SetActive(false);
        roundEndPanel.SetActive(false);
        _roundStartString = roundStartText.text;
        foreach (TextMeshProUGUI text in roundEndPanel.GetComponentsInChildren<TextMeshProUGUI>())
        {
            startStringsEndPanel.Add(text, text.text);
        }
        _roundEndString = roundEndText.text;
        roundEndText.text = roundEndText.text.Replace("[TIME]", EnemyManager.WaveDelay.ToString());
        EnemyManager.OnRoundStarted.AddListener(_OnRoundStart);
        EnemyManager.OnRoundEnded.AddListener(_OnRoundEnd);
    }
    private void OnDestroy()
    {
        EnemyManager.OnRoundStarted.RemoveListener(_OnRoundStart);
        EnemyManager.OnRoundEnded.RemoveListener(_OnRoundEnd);
    }

    void _OnRoundStart()
    {
        StartCoroutine(fadeGOAfter(roundStartPanel));
    }

    void _OnRoundEnd()
    {
        if (EnemyManager.IsWaveValid(EnemyManager.instance._public_wave_i + 1) == false)
            return;
        StartCoroutine(fadeGOAfter(roundEndPanel));
    }
    void _RefreshTexts()
    {
        roundStartText.text = _FormatString(_roundStartString);
        foreach (var t in startStringsEndPanel)
        {
            TextMeshProUGUI text = t.Key;
            text.text = _FormatString(t.Value);
        }
        //roundEndText.text = _FormatString(_roundEndString);
    }
    string _FormatString(string start)
    {
        start = start.Replace("[N]", EnemyManager.CurrentWave.ToString());
        start = start.Replace("[N - 1]", (EnemyManager.CurrentWave - 1).ToString());
        start = start.Replace("[TIME]", EnemyManager.TimeUntilNextWave.ToString());
        start = start.Replace("[$]", EnemyManager.LastWaveBonus.ToString());


        return start;
    }
    Transform player => InventoryManager.instance.playerTransform;
    Transform playerCam => InventoryManager.instance.playerCameraTransform;
    /// <summary>
    /// Spawns a copy of the given GO, and destroys it after a few seconds
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    IEnumerator fadeGOAfter(GameObject go)
    {
        
        yield return new WaitForSeconds(.1f);
        Vector3 dir = playerCam.transform.forward;
        dir.y = 0; dir = dir.normalized * distanceFromPlayer;
        var pos = player.transform.position + dir;
        pos += Vector3.up * height;

        _RefreshTexts();
        GameObject spawned = Instantiate(go, transform);
        spawned.SetActive(true);
        spawned.transform.position = pos;
        spawned.transform.LookAt(pos + dir);
        //spawned.transform.LookAt(pos - dir);
        Destroy(spawned, displayTime);
        //go.SetActive(true);
        //yield return new WaitForSeconds(displayTime);
        //go.SetActive(false);
        yield break;
    }
}
