using System.Collections;
using System.Linq;
using TMPro;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

public class StatDisplayController : MonoBehaviour
{
    [FormerlySerializedAs("statTracker")] [SerializeField] private StatDisplayModel statDisplayModel;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text valueText;

    [SerializeField] private Animator animator;

    [SerializeField] private PlayableAsset TowerBuildTimeline;
    PlayableDirector TowerDirector { get {
            if (_towerDirector == null && _checkedTowerDirector == false) 
            {
                XRGrabInteractable grab = GetComponentInParent<XRGrabInteractable>();
                if (grab != null)
                {
                    foreach (PlayableDirector director in grab.GetComponentsInChildren<PlayableDirector>())
                    {
                        if (director.playableAsset == TowerBuildTimeline) _towerDirector = director;
                    }
                }
                _checkedTowerDirector = true;
            }
            return _towerDirector;
        } }
    bool _checkedTowerDirector = false;
    PlayableDirector _towerDirector = null;

    private void OnEnable()
    {
        //foreach (var stat in statDisplayModel.statTrackers)
        //    stat.SerializeIfChanged();

        _startPos = transform.parent.position;
        _startRot = transform.parent.rotation;

        if(statDisplayModel.statTrackers.All(stat => stat.getSerializeValue == 0))
            Destroy(transform.parent.gameObject);
        titleText.text = statDisplayModel.displayName;

        foreach (var tracker in statDisplayModel.statTrackers)
        {
            var text = Instantiate(valueText, titleText.transform.parent);
            text.text = $"{tracker.statName}: {tracker.getSerializeValue}";
            if (tracker.displayTextColor != Color.clear)
                text.color = tracker.displayTextColor;
            if (tracker is TowerDestroyedTracker towerDestroyedTracker)
            {
                //text = Instantiate(valueText, titleText.transform.parent);
                text.text += $"\t{towerDestroyedTracker.LostAsPlayerSuffix}: {towerDestroyedTracker.DestroyedAsPlayerCount}";
            }
        }

        XRGrabInteractable grab = GetComponentInParent<XRGrabInteractable>();
        grab.firstSelectEntered.AddListener(OnPickupItem);
        grab.lastSelectExited.AddListener(OnDropItem);

        grab.firstHoverEntered.AddListener(_OnHoverStart);
        grab.lastHoverExited.AddListener(_OnHoverEnd);

        if (animator != null)
            _CloseDisplay();

    }
    Vector3 _startPos;
    Quaternion _startRot;
    public bool ShouldResetAfterDrop = false;
    public float ResetAfterDropTime = 2f;
    void _OnHoverStart(HoverEnterEventArgs a)
    {
        if (_held) return;
        _OpenDisplay();
    }
    void _OnHoverEnd(HoverExitEventArgs a)
    {
        if (_held) return;
        RestartBuildAfter(1f);
        _CloseDisplay();

    }
    bool _held = false;
    public void OnPickupItem(SelectEnterEventArgs a)
    {
        _OpenDisplay();
        _held = true;
    }

    public void OnDropItem(SelectExitEventArgs a)
    {
        _held = false;
        _CloseDisplay();
        if (_currentResetter != null) StopCoroutine(_currentResetter );
        _currentResetter = _ResetAfterDrop();
        StartCoroutine(_currentResetter);
    }
    IEnumerator _currentResetter = null;
    IEnumerator _ResetAfterDrop()
    {
        float t = 0f;
        while (t < ResetAfterDropTime)
        {
            if (ShouldResetAfterDrop == false || _held) yield break;
            yield return null;
            t += Time.deltaTime;
            CancelBuildAnimDelay();


        }
        transform.parent.position = _startPos;
        transform.parent.rotation = _startRot;
        RestartBuildAfter(1f);
        _currentResetter = null;
    }
    void _OpenDisplay() { animator.Play("OpenDisplay"); CancelBuildAnimDelay(); }
    void _CloseDisplay() { 
        animator.Play("CloseDisplay");
        
    }
    void CancelBuildAnimDelay() { 
        if (_currentDelayRoutine != null) 
            StopCoroutine(_currentDelayRoutine); 
        _currentDelayRoutine = null; }
    void RestartBuildAfter(float time = 0f)
    {
        if (gameObject.activeInHierarchy == false) return;
        if (_currentDelayRoutine != null) StopCoroutine(_currentDelayRoutine);
        _currentDelayRoutine = delayAnim(time);
        StartCoroutine(_currentDelayRoutine);
        
    }
    IEnumerator _currentDelayRoutine = null;
    IEnumerator delayAnim(float time = 0f)
    {
        yield return new WaitForSeconds(time);
        _RestartBuildAnim();
        _currentDelayRoutine = null;
    }
    void _RestartBuildAnim()
    {
        //if (TowerDirector == null) { Debug.Log($"No TowerDirector on me: {gameObject.name}", this); }
        if (TowerDirector == null) return;
        //Debug.Log($"Restarting director {TowerDirector.gameObject.name}", TowerDirector);
        TowerDirector.Stop();
        TowerDirector.time = 0;
        TowerDirector.Play();
        
    }

}