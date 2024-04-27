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
            timeSpentFreeMoving += Time.deltaTime;
            yield return null;

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


    void _OnTeleport(LocomotionSystem system)
    {
        TutorialManager.SetSkip(true);
        TutorialManager.RecenterGUI();
    }

    IEnumerator _currentLockRemover = null;
    IEnumerator _RemoveLockAfter()
    {
        yield return new WaitForSeconds(RemoveMoveLockTime);
        DynamicMoveProvider.RemoveMovementLock();

        input.started += FreeMovePressed;
        input.canceled += FreeMoveReleased;

        
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
        print($"Player took control of {tower.dto.name}");
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
    void _OnPlayerChangeState(PlayerState oldState, PlayerState newState)
    {
        if (newState == PlayerState.IDLE)
        {
            _Skip();
            PlayerStateController.OnStateChange -= _OnPlayerChangeState;
        }
    }
    

    void _Skip() { TutorialManager.SetSkip(); }

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

}
