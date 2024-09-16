using Project.Towers.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class TutorialPanel : MonoBehaviour
{
    public _WaitUntilAction WaitUntilAction = _WaitUntilAction.None;
    [SerializeField] InputActionReference capturedInput;
    public int PressThreshold = 3;
    public float RemoveMoveLockTime = 3f;
    [Tooltip("How long the player has to spend free moving before moving on")]
    public float moveTimeThreshold = 2f;
    InputAction input => Utilities.GetInputAction(capturedInput);


    public List<Tower_SO> targetToSpawn = new List<Tower_SO>();
    public Tower_SO tower_upgrade;
    public TextMeshProUGUI displayText;
    public Color activeColor;
    public Color inactiveColor;
    HashSet<Tower_SO> _towerTargets;
    // Start is called before the first frame update
    void Start()
    {
        _towerTargets = targetToSpawn.ToHashSet();
    }
    private void OnEnable()
    {
        if (WaitUntilAction == _WaitUntilAction.Teleport
            && TutorialManager.tp != null)
        {
            displayText.text = $"0 / {PressThreshold}";
            displayText.color = inactiveColor;
            TutorialManager.tp.beginLocomotion += _OnTeleport;
        }

        if (WaitUntilAction == _WaitUntilAction.QuickLook)
        {
            input.performed += Input_performed;
        }

        if (WaitUntilAction == _WaitUntilAction.FreeMove)
        {
            _currentLockRemover = _RemoveLockAfter();
            StartCoroutine(_currentLockRemover);
        }
        if (WaitUntilAction == _WaitUntilAction.TowerPlacer)
        {
            Tower.OnTowerSpawn += _OnTowerSpawn;
            _UpdateTowerText();
        }
        if (WaitUntilAction == _WaitUntilAction.TowerSelect)
        {
            Tower.OnStartFocus += _OnTowerSelect;
        }
        if (WaitUntilAction == _WaitUntilAction.Takeover)
            PlayerStateController.instance.OnPlayerTakeoverTower += _OnTowerTakeover;
        if (WaitUntilAction == _WaitUntilAction.QuickTakeover)
        {
            PlayerStateController.instance.OnPlayerQuickTakeoverTower += _OnTowerQuickTakeover;
        }
        if (WaitUntilAction == _WaitUntilAction.Upgrade)
        {
            TowerSpawnManager.OnTowerUpgraded += _OnTowerUpgraded;
        }
        if (WaitUntilAction == _WaitUntilAction.LeaveTower)
        {
            PlayerStateController.OnStateChange += _OnPlayerChangeState;
        }
        if (WaitUntilAction == _WaitUntilAction.SkipRound)
        {
            input.started += SkipPressed;
            EnemyManager.instance.SKIP_TUTORIAL_IS_COMPLETE = true;
        }
        if (WaitUntilAction == _WaitUntilAction.GuidedMissile)
        {
            CombatTutorial.DummyParent.SetActive(true);
            GuidedMissileController.OnMissileFiredAt += _OnMissileFiredAt;
        }
        if (WaitUntilAction == _WaitUntilAction.PressB)
        {
            capturedInput.action.started += _OnPressB;
            _isListeningForB = true;
        }
        if (WaitUntilAction == _WaitUntilAction.Pause)
            capturedInput.action.started += _OnPausePressed;

        if (WaitUntilAction == _WaitUntilAction.BasicSkip)
        {
            XRSimpleInteractable simple = GetComponentInChildren<XRSimpleInteractable>();
            simple.activated.AddListener(_WelcomeToCastlePressed);
        }
        
    }

    private void SkipPressed(InputAction.CallbackContext obj)
    {
        _Skip();
    }

    private void OnDisable()
    {
        if (WaitUntilAction == _WaitUntilAction.Teleport
            && TutorialManager.tp != null)
        {
            TutorialManager.tp.beginLocomotion -= _OnTeleport;
        }
        if (WaitUntilAction == _WaitUntilAction.QuickLook)
        {
            input.performed -= Input_performed;
        }
        if (WaitUntilAction == _WaitUntilAction.FreeMove)
        {
            if (_currentLockRemover != null)
                StopCoroutine(_currentLockRemover);
        }
        if (WaitUntilAction == _WaitUntilAction.TowerPlacer)
        {
            Tower.OnTowerSpawn -= _OnTowerSpawn;
        }
        if (WaitUntilAction == _WaitUntilAction.TowerSelect)
        {
            Tower.OnStartFocus -= _OnTowerSelect;
        }
        if (WaitUntilAction == _WaitUntilAction.GuidedMissile)
        {
            CombatTutorial.DummyParent.SetActive(false);
            GuidedMissileController.OnMissileFiredAt -= _OnMissileFiredAt;
        }
        if (WaitUntilAction == _WaitUntilAction.PressB && _isListeningForB)
        {
            capturedInput.action.started -= _OnPressB;
        }
        if (WaitUntilAction == _WaitUntilAction.Pause)
            capturedInput.action.started -= _OnPausePressed;

    }
    int _pausePresses = 0;
    void _OnPausePressed(InputAction.CallbackContext obj)
    {
        _pausePresses++;
        if (_pausePresses >= PressThreshold) return;
        displayText.color = activeColor;
        StartCoroutine(_SkipAfter());
    }
    int presses = 0;
    private void Input_performed(InputAction.CallbackContext obj)
    {
        presses++;
        
        if (presses >= PressThreshold)
        {
            StartCoroutine(_SkipAfter());
        }
        if (WaitUntilAction == _WaitUntilAction.QuickLook)
        {
            displayText.text = $"{presses} / {PressThreshold}";
            if (presses >= PressThreshold)
                displayText.color = activeColor;
        }
        StartCoroutine(_RecenterAfter());
    }
    bool _hasSkipped = false;
    IEnumerator _SkipAfter(float t = 2f)
    {
        if (_hasSkipped) yield break;
        _hasSkipped = true;
        yield return new WaitForSeconds(t);
        TutorialManager.SetSkip(true);
    }
    void FreeMovePressed(InputAction.CallbackContext obj)
    {
        _StopMover();
        _currentMover = _FreeMoving();
        StartCoroutine(_currentMover);
    }
    void _StopMover() { if (_currentMover != null) StopCoroutine(_currentMover); _currentMover = null; }
    IEnumerator _currentMover = null;
    float timeSpentFreeMoving = 0f;
    IEnumerator _FreeMoving()
    {
        while (true)
        {
            if (DynamicMoveProvider.canMove == false) 
            { 
                yield return null;
                continue;
            }
            timeSpentFreeMoving += Time.deltaTime;
            yield return null;
            float percent = timeSpentFreeMoving / moveTimeThreshold;
            int i = Mathf.FloorToInt(percent * 10f);
            string dots = string.Concat(Enumerable.Repeat(".", 10 - i));
            string xs = string.Concat(Enumerable.Repeat("X", i));
            displayText.text = $"[{xs}{dots}]";
            if (timeSpentFreeMoving >= moveTimeThreshold)
            {
                TutorialManager.SetSkip(true);
                input.started -= FreeMovePressed;
                input.canceled -= FreeMoveReleased;
                yield break;
            }
        }
    }
    void FreeMoveReleased(InputAction.CallbackContext obj)
    {
        _StopMover();
    }
    IEnumerator _RecenterAfter()
    {
        yield return new WaitForSeconds(0.3f);
        TutorialManager.RecenterGUI();
    }


    int _teleports = 0;
    void _OnTeleport(LocomotionSystem system)
    {
        _teleports++;
        displayText.color = inactiveColor;
        displayText.text = $"{_teleports} / {PressThreshold}";
        TutorialManager.RecenterGUI();
        if (_teleports == PressThreshold)
        {
            TutorialManager.SetSkip(true);

            displayText.color = activeColor;
        }
        
    }

    IEnumerator _currentLockRemover = null;
    IEnumerator _RemoveLockAfter()
    {
        input.started += FreeMovePressed;
        input.canceled += FreeMoveReleased;


        yield return new WaitForSeconds(RemoveMoveLockTime);
        DynamicMoveProvider.RemoveMovementLock();

        

        
        _currentLockRemover = null;
    }
    HashSet<Tower_SO> spawned = new HashSet<Tower_SO>();
    void _OnTowerSpawn(Tower tower)
    {
        var dto = tower.dto;
        //If we haven't spawned one yet, but we're supposed to, add it to the list
        if (spawned.Contains(dto) == false && _towerTargets.Contains(dto))
        {
            spawned.Add(dto);
        }
        _UpdateTowerText();
        //We've spawned all the towers we need to
        if (spawned.Count == _towerTargets.Count)
        {
            Tower.OnTowerSpawn -= _OnTowerSpawn;
            TutorialManager.SetSkip();
        }
    }
    void _UpdateTowerText()
    {
        string s = "";
        foreach (var dto in targetToSpawn)
        {
            var c = (spawned.Contains(dto)) ? activeColor : inactiveColor;
            string color = ColorUtility.ToHtmlStringRGBA(c);
            s += $"- <color=#{color}>{dto.name}</color>\n";

        }
        s = s.Trim();
        displayText.text = s;
    }
    void _OnTowerSelect(Tower tower)
    {
        TutorialManager.SetSkip();
    }

    void _OnTowerTakeover(PlayerControllableTower tower)
    {
        _Skip();
        PlayerStateController.instance.OnPlayerTakeoverTower -= _OnTowerTakeover;
    }
    void _OnTowerQuickTakeover(PlayerControllableTower tower)
    {
        _Skip();
        PlayerStateController.instance.OnPlayerQuickTakeoverTower -= _OnTowerQuickTakeover;
    }
    void _OnTowerUpgraded(Tower_SO dto)
    {
        if (dto != tower_upgrade) return;
        _Skip();
        TowerSpawnManager.OnTowerUpgraded -= _OnTowerUpgraded;
    }
    int _guided_missile_shots = 0;
    void _OnMissileFiredAt(Enemy e)
    {
        if (e == null) return;
        _guided_missile_shots++;
        displayText.text = $"{_guided_missile_shots} / 3";
        if (_guided_missile_shots >= 3)
        {
            displayText.color = activeColor;
            StartCoroutine(_SkipAfter(3f));
        }
    }
    bool _isListeningForB = false;
    void _OnPressB(InputAction.CallbackContext context)
    {
        displayText.color = activeColor;
        _isListeningForB = false;
        capturedInput.action.started -= _OnPressB;
        StartCoroutine(_SkipAfter());
    }

    void _OnPlayerChangeState(PlayerState oldState, PlayerState newState)
    {
        if (newState == PlayerState.IDLE)
        {
            _Skip();
            PlayerStateController.OnStateChange -= _OnPlayerChangeState;
        }
    }
    

    void _Skip() { TutorialManager.SetSkip(); }

    void _WelcomeToCastlePressed(ActivateEventArgs a) { TutorialManager.SetSkip(); }

}

public enum _WaitUntilAction
{
    None,
    Teleport,
    QuickLook,
    FreeMove,
    TowerPlacer,
    TowerSelect,
    Takeover,
    QuickTakeover,
    Upgrade,
    LeaveTower,
    SkipRound,
    GuidedMissile,
    PressB,
    Pause,
    BasicSkip,
    GrabCannonball,

}
