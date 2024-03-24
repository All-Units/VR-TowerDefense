using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class RoundGUI : MonoBehaviour
{
    [SerializeField] private PlayableDirector roundStartPanel;
    [SerializeField] private PlayableDirector roundEndPanel;
    [SerializeField] private TextMeshProUGUI roundStartText;
    [SerializeField] private float displayTime = 3f;
    [SerializeField] private float distanceFromPlayer = 3f;
    private string _roundStartString;
    private string _roundEndString;

    private readonly Dictionary<TextMeshProUGUI, string> _startStringsEndPanel = new();

    // Start is called before the first frame update
    private void Start()
    {
        roundStartPanel.GetComponent<RectTransform>().localScale = Vector3.zero;
        roundEndPanel.GetComponent<RectTransform>().localScale = Vector3.zero;
        
        _roundStartString = roundStartText.text;
        foreach (var text in roundEndPanel.GetComponentsInChildren<TextMeshProUGUI>())
        {
            _startStringsEndPanel.Add(text, text.text);
        }
        
        EnemyManager.OnRoundStarted.AddListener(_OnRoundStart);
        EnemyManager.OnRoundEnded.AddListener(_OnRoundEnd);
    }
    private void OnDestroy()
    {
        EnemyManager.OnRoundStarted.RemoveListener(_OnRoundStart);
        EnemyManager.OnRoundEnded.RemoveListener(_OnRoundEnd);
    }

    private void _OnRoundStart()
    {
        _DisplayPanel(roundStartPanel);
    }

    private void _OnRoundEnd()
    {
        if (EnemyManager.IsWaveValid(EnemyManager.instance._public_wave_i + 1) == false)
            return;
        _DisplayPanel(roundEndPanel);
    }

    private void _DisplayPanel(PlayableDirector panel)
    {
        _RepositionPanel(panel.gameObject);
        _RefreshTexts();
        panel.Play();
    }

    private void _RefreshTexts()
    {
        roundStartText.text = _FormatString(_roundStartString);
        foreach (var (text, s) in _startStringsEndPanel)
        {
            text.text = _FormatString(s);
        }
    }

    private string _FormatString(string start)
    {
        start = start.Replace("[N]", EnemyManager.CurrentWave.ToString());
        start = start.Replace("[N - 1]", (EnemyManager.CurrentWave - 1).ToString());
        start = start.Replace("[TIME]", EnemyManager.TimeUntilNextWave.ToString());
        start = start.Replace("[$]", EnemyManager.LastWaveBonus.ToString());

        return start;
    }

    private void _RepositionPanel(GameObject panel)
    {
        if (InventoryManager.instance == null) return;
        var cam = InventoryManager.instance.playerCameraTransform;


        var camTransform = cam.transform;
        var dir = camTransform.forward; dir.y = 0f;
        dir = dir.normalized * distanceFromPlayer;
        var center = camTransform.position + dir;
        panel.transform.position = center;

        var bottom = panel.transform.Find("bottom");
        if (bottom == null) { Debug.LogError("no bottom!"); return;}
        var pos = center + dir;
        LayerMask mask = LayerMask.GetMask("Ground");
        if (Physics.Raycast(pos + Vector3.up * 100f, Vector3.down, out var hit, float.PositiveInfinity, mask))
        {
            //print($"HIT Y: {hit.point.y}, bottom y was {bottom.position.y}");
            if (hit.point.y > bottom.position.y)
            {
                //float offset = Mathf.Max(height, 0.3f);
                var offset = 0.3f;
                offset += Mathf.Abs(hit.point.y - bottom.position.y);
                panel.transform.Translate(new Vector3(0f, offset, 0f));
                // print($"Panel was too low, moving up to {panel.transform.position.y}");
            }
            //if (hit.point.y > canvasPos.y)
        }
        var target = pos + dir;
        target.y = panel.transform.position.y;
        panel.transform.LookAt(target);
    }
    
    /// <summary>
    /// Spawns a copy of the given GO, and destroys it after a few seconds
    ///
    /// Why?!? 
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    private IEnumerator FadeGOAfter(GameObject go)
    {

        _RefreshTexts();
        var spawned = Instantiate(go, transform);
        spawned.SetActive(true);
        _RepositionPanel(spawned);
        
        //spawned.transform.position = pos;
        //spawned.transform.LookAt(pos + dir);
        //spawned.transform.LookAt(pos - dir);
        spawned.DestroyAfter(displayTime);
        //Destroy(spawned, displayTime);
        
        yield break;
    }
}
