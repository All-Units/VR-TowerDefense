using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class TutorialManager : MonoBehaviour
{
    public GameObject GUI_Parent;
    [SerializeField] PlayableAsset growTimeline;
    [SerializeField] PlayableAsset shrinkTimeline;
    public List<_PanelDisplayTime> firstDisplayTimes = new List<_PanelDisplayTime>();
    public List<_PanelDisplayTime> castleTutorials = new List<_PanelDisplayTime>();
    public List<_PanelDisplayTime> secondWaveTutorials = new List<_PanelDisplayTime>();
    Transform gui => GUI_Parent.transform;
    PlayableDirector director;
    public float _freeMoveRecenterThreshold = 3f;
    [SerializeField] InputActionReference moveInput;
    InputAction input => Utilities.GetInputAction(moveInput);
    static TutorialManager instance;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        director = GUI_Parent.GetComponentInChildren<PlayableDirector>();
        EnemyManager.instance.IS_TUTORIAL = true;
        StartCoroutine(_DisplayList(firstDisplayTimes));

        DynamicMoveProvider.AddMovementLock();
        CurrencyManager.OnChangeMoneyAmount += _EnsureMinimumCash;

        tp.endLocomotion += _RecenterOnTP;
        input.started += Input_started;
        input.canceled += Input_canceled;

        GameStateManager.onGameWin += _OnWin;

        EnemyManager.OnRoundEnded.AddListener(_OnFirstRoundEnd);

        PlayerPrefs.SetInt("_has_completed_tutorial", 0);
    }
    private void OnDestroy()
    {
        input.started -= Input_started;
        input.canceled -= Input_canceled;
        CurrencyManager.OnChangeMoneyAmount -= _EnsureMinimumCash;
        GameStateManager.onGameWin -= _OnWin;
        instance = null;
        
    }

    private void Input_canceled(InputAction.CallbackContext obj)
    {
        if (DynamicMoveProvider.canMove == false) return;
        _moveHeld = false;
    }
    bool _moveHeld = false;
    private void Input_started(InputAction.CallbackContext obj)
    {
        if (DynamicMoveProvider.canMove == false) return;
        _moveHeld = true;
        StartCoroutine(_TrackMovement());
    }
    IEnumerator _currentMoveTracker = null;
    IEnumerator _TrackMovement()
    {
        Vector3 _lastPos = cam.position; _lastPos.y = 0f;
        float distance = 0f;
        while (_moveHeld)
        {
            yield return null;
            Vector3 pos = cam.position; pos.y = 0f;
            distance += Vector3.Distance(_lastPos, pos);
            if (distance >= _freeMoveRecenterThreshold)
            {
                _RecenterGUI();
                distance = 0f;
            }
            _lastPos = pos;
        }
    }
    public int MinimumCash = 100;
    
    void _EnsureMinimumCash(int cash)
    {
        if (cash < MinimumCash)
            CurrencyManager.GiveToPlayer(150);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            _RecenterGUI();
    }
    Transform cam => InventoryManager.instance.playerCameraTransform;
    public static void RecenterGUI() { instance._RecenterGUI(); }
    void _RecenterGUI()
    {
        gui.position = cam.position;
        //Vector3 euler = new Vector3(0f, cam.eulerAngles.y, 0f);
        //gui.eulerAngles = euler;
    }
    void _OnFirstRoundEnd()
    {
        StartCoroutine(_DisplayList(secondWaveTutorials));
        EnemyManager.OnRoundEnded.RemoveListener(_OnFirstRoundEnd);
    }

    IEnumerator _DisplayList(List<_PanelDisplayTime> panels)
    {
        foreach (var panelDisplay in panels)
            panelDisplay.gameObject.SetActive(false);
        foreach (var panelDisplay in panels)
        {
            yield return new WaitForSeconds(panelDisplay.waitBeforeTime);
            _RecenterGUI();
            panelDisplay.gameObject.SetActive(true);
            director.playableAsset = growTimeline;
            director.Play();
            yield return new WaitForSeconds(growTimeline.duration());
            float t = 0f;
            while (t <= panelDisplay.displayTime)
            {
                if (panelDisplay.waitForSignal == false)
                    t += Time.deltaTime;
                yield return null;
                if (_skip)
                    break;
            }
            _skip = false;
            if (_skip_to_main_menu) yield break;
            director.playableAsset = shrinkTimeline;
            director.Play();
            yield return new WaitForSeconds(shrinkTimeline.duration());

            panelDisplay.gameObject.SetActive(false);
            if (_skip_movement)
            {
                _skip_movement = false;
                break; }

        }
        //If we were the first list, start the second
        if (panels.FirstOrDefault().gameObject == firstDisplayTimes.FirstOrDefault().gameObject)
        {
            StartCoroutine(_DisplayList(castleTutorials));
        }
        //If we were the second list, start real combat
        else if (panels.FirstOrDefault().gameObject == castleTutorials.FirstOrDefault().gameObject)
        {
            EnemyManager.SkipToNextRound = true;
        }
    }
    public static void SetSkip(bool skip = true)
    {
        instance._skip = skip;
    }
    bool _skip = false;
    public void NewToVR()
    {
        _skip = true;
    }

    bool _skip_movement = false;
    public void SkipMovement()
    {
        _skip = true;
        _skip_movement = true;
        DynamicMoveProvider.RemoveMovementLock();
    }
    bool _skip_to_main_menu = false;
    public void SkipTutorial()
    {
        _skip = true;
        _skip_to_main_menu = true;
        _OnWin();
        XRPauseMenu.MainMenu();
    }
    void _RecenterOnTP(LocomotionSystem system)
    {
        StartCoroutine(_RecenterAfter());
    }
    IEnumerator _RecenterAfter(float time = 0.2f)
    {
        yield return new WaitForSeconds(time);
        _RecenterGUI();
    }

    
    void _OnWin()
    {
        PlayerPrefs.SetInt("_has_completed_tutorial", 1);
    }


    public static TeleportationProvider tp {  get {
        if (instance == null || InventoryManager.instance == null) return null;
        if (instance._tp == null)
            instance._tp = InventoryManager.instance.GetComponentInChildren<TeleportationProvider>();
        return instance._tp; } }
    TeleportationProvider _tp = null;
}

[System.Serializable]
public struct _PanelDisplayTime
{
    public GameObject panel;
    public GameObject gameObject => panel;
    public float displayTime;
    public float waitBeforeTime;
    public bool waitForSignal;
}