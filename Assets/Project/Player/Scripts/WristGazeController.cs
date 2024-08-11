using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR.Interaction.Toolkit;

public class WristGazeController : MonoBehaviour
{
    [SerializeField] XRSimpleInteractable gazeInteractor;
    [SerializeField] GameObject WristPanel;
    [SerializeField] float lookAwayGracePeriod = 1f;
    [SerializeField] PlayableAsset growTimeline;
    [SerializeField] PlayableAsset shrinkTimeline;

    [Header("Text References")]
    [SerializeField] TextMeshProUGUI waveNumberText;
    [SerializeField] TextMeshProUGUI enemyText;
    [SerializeField] TextMeshProUGUI cashText;

    string startWaveText, startEnemyText, startCashText;

    PlayableDirector guiDirector;
    // Start is called before the first frame update
    void Start()
    {
        WristPanel.SetActive(false);
        guiDirector = WristPanel.GetComponent<PlayableDirector>();

        gazeInteractor.hoverEntered.AddListener(LookAt);
        gazeInteractor.hoverExited.AddListener(LookAway);
        startWaveText = waveNumberText.text;
        startEnemyText = enemyText.text;
        startCashText = cashText.text;

        _SetWave(1);

        EnemyManager.OnRoundEnded.AddListener(_OnWaveEnd);

        EnemyManager.instance.OnEnemySpawned.AddListener(_OnEnemyChange);
        EnemyManager.instance.OnEnemyKilled.AddListener(_OnEnemyChange);
        _OnEnemyChange();

        CurrencyManager.OnChangeMoneyAmount += _OnCurrencyChange;
        StartCoroutine(_SetCashDelay());
        haptics = GetComponentInParent<ActionBasedController>();
        canvas.SetActive(false);
    }
    public GameObject canvas;
    public ActionBasedController leftHaptics;
    ActionBasedController haptics;
    void Buzz(float magnitude = 1f, float duration = 0.1f)
    {
        if (haptics == null) return;
        haptics.SendHapticImpulse(magnitude, duration);
    }
    void LeftBuzz(float magnitude = 1f, float duration = 0.1f)
    {
        if (leftHaptics == null) return;
        leftHaptics.SendHapticImpulse(magnitude, duration);
    }
   

    IEnumerator _SetCashDelay()
    {
        yield return null;
        yield return null;
        _OnCurrencyChange(CurrencyManager.CurrentCash);
    }
    int _wave = 1;
    void _OnWaveEnd()
    {
        _wave++;
        _SetWave(_wave);
    }
    void _SetWave(int i)
    {
        string text = startWaveText.Replace("[N]", i.ToString());
        _SetText(waveNumberText, text);
    }
    void _SetText(TextMeshProUGUI text, string s) { text.text = s; }
    

    void _OnEnemyChange()
    {
        int enemies = EnemyManager.CurrentEnemyCount;
        string text = startEnemyText.Replace("[E]", enemies.ToString());
        _SetText(enemyText, text);
    }
    
    void _OnCurrencyChange(int current)
    {
        string text = startCashText.Replace("[$]", current.ToString());
        _SetText(cashText, text);
    }

    /// <summary>
    /// When the player begins looking at their wrist
    /// </summary>
    /// <param name="a"></param>
    void LookAt(HoverEnterEventArgs a)
    {
        Buzz(1f, 0.1f);
        bool _wasLooking = _IsLookingAt;
        _IsLookingAt = true;
        canvas.SetActive(true);
        return;
        StartCoroutine(_LookAtAfter(_wasLooking));
    }
    IEnumerator _LookAtAfter(bool wasLooking)
    {
        yield return new WaitForSeconds(lookAwayGracePeriod);
        if (_IsLookingAt == false) yield break;
        WristPanel.SetActive(true);
        guiDirector.playableAsset = growTimeline;
        if (wasLooking == false)
        {
            guiDirector.Play();
        }
        _StopDisableIfExists();
    }
    
    /// <summary>
    /// When the player looks away from their wrist
    /// </summary>
    /// <param name="a"></param>
    void LookAway(HoverExitEventArgs a)
    {
        LeftBuzz();
        canvas.SetActive(false);
        _StopDisableIfExists();
        _IsLookingAt = false;
        StartCoroutine(_DisableAfter());
    }

    bool _IsLookingAt = false;
    IEnumerator _currentDisableRoutine = null;
    void _StopDisableIfExists()
    {
        if (_currentDisableRoutine != null)
            StopCoroutine( _currentDisableRoutine );
        _currentDisableRoutine = null;
    }
    IEnumerator _DisableAfter()
    {
        float t = 0f;
        while (t <= lookAwayGracePeriod)
        {
            //If we start looking again, break
            if (_IsLookingAt)
            {
                yield break;
            }
            yield return null;
            t += Time.deltaTime;
        }
        guiDirector.playableAsset = shrinkTimeline;
        guiDirector.Play();
        float duration = 0.5f;
        if (shrinkTimeline != null) duration = (float)shrinkTimeline.duration;
        t = 0f;
        while (t <= duration)
        {
            //If we start looking again, break
            if (_IsLookingAt)
            {
                yield break;
            }
            yield return null;
            t += Time.deltaTime;
        }
        WristPanel.SetActive(false);
        _currentDisableRoutine = null;
    }
}
