using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class RoundGUI : MonoBehaviour
{
    [SerializeField] private PlayableDirector roundStartPanel;
    [SerializeField] private PlayableDirector roundEndPanel;
    [SerializeField] private PlayableDirector victoryPanel;
    [SerializeField] private PlayableDirector defeatPanel;
    
    [SerializeField] private TextMeshProUGUI roundStartText;
    [SerializeField] private float displayTime = 3f;
    [SerializeField] private float distanceFromPlayer = 3f;
    private string _roundStartString;
    private string _roundEndString;

    private readonly Dictionary<TextMeshProUGUI, string> _startStringsEndPanel = new();

    // Start is called before the first frame update
    private void Start()
    {
        _roundStartString = roundStartText.text;
        foreach (var text in roundEndPanel.GetComponentsInChildren<TextMeshProUGUI>())
        {
            _startStringsEndPanel.Add(text, text.text);
        }
        
        EnemyManager.OnRoundStarted.AddListener(_OnRoundStart);
        EnemyManager.OnRoundEnded.AddListener(_OnRoundEnd);
        
        GameStateManager.onStartGameWin += OnStartGameWin;
        GameStateManager.onStartGameLose += OnStartGameLose;
    }

    private void OnStartGameWin()
    {
        _DisplayPanel(victoryPanel);
    }
    
    private void OnStartGameLose()
    {
        _DisplayPanel(defeatPanel);
    }

    private void OnDestroy()
    {
        EnemyManager.OnRoundStarted.RemoveListener(_OnRoundStart);
        EnemyManager.OnRoundEnded.RemoveListener(_OnRoundEnd);
    }

    private void _OnRoundStart()
    {
        _RefreshTexts();
        _DisplayPanel(roundStartPanel);
    }

    private void _OnRoundEnd()
    {
        if (EnemyManager.IsWaveValid(EnemyManager._public_wave_i + 1) == false)
            return;
        _RefreshTexts();
        _DisplayPanel(roundEndPanel);
    }

    private void _DisplayPanel(PlayableDirector panel)
    {
        if (panel == null) return;
        try
        {
            _RepositionPanel(panel.gameObject);
            panel.Play();
        }
        catch (MissingReferenceException e) { }        
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
        // This should be directly assigned
        start = start.Replace("[N]", EnemyManager.CurrentWave.ToString());
        start = start.Replace("[N - 1]", (EnemyManager.CurrentWave).ToString());
        start = start.Replace("[TIME]", EnemyManager.TimeUntilNextWave.ToString());
        start = start.Replace("[$]", EnemyManager.LastWaveBonus.ToString());

        return start;
    }

    private void _RepositionPanel(GameObject panel)
    {
        if (InventoryManager.instance == null) return;
        var cam = InventoryManager.instance.playerCameraTransform;


        var camTransform = cam.transform;
        var dir = camTransform.forward;
        dir.y = 0f;
        dir = dir.normalized * distanceFromPlayer;
        var center = camTransform.position + dir;
        panel.transform.position = center;

        var bottom = panel.transform.Find("bottom");
        if (bottom == null)
        {
            Debug.LogError("no bottom!");
            return;
        }

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
}
