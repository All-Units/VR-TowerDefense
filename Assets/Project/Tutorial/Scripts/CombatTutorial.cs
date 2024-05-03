using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;

public class CombatTutorial : MonoBehaviour
{
    [Header("Progress vars")]
    public TextMeshProUGUI progressText;
    public GameObject progressPanel;

    [SerializeField] GameObject archerPanel;
    public TextMeshProUGUI archerCount;
    public int ArcherShotsRequired = 5;
    [SerializeField] GameObject cannonPanel;
    public TextMeshProUGUI cannonCountText;
    public int CannonShotsRequired = 5;
    [SerializeField] GameObject magePanel;
    public TextMeshProUGUI mageCountText;
    public int mageShotsRequired = 5;

    public GameObject dummyParent;
    public static GameObject DummyParent;

    public Tower_SO archerDTO, cannonDTO, mageDTO;

    public Color activeColor, inactiveColor;
    GameObject lastPanel = null;
    private void OnEnable()
    {
        if (PlayerStateController.instance == null) return;
        var currentTower = PlayerStateController.CurrentTower;
        //if (currentTower == null) return;

        DummyParent = dummyParent;
        dummyParent.SetActive(true);
        _OnPlayerTakeover(currentTower);

        PlayerStateController.instance.OnPlayerTakeoverTower += _OnPlayerTakeover;
        PlayerStateController.OnStateChange += PlayerStateController_OnStateChange;
        FireballGlovesController.OnFireballShoot += _OnFireballThrow;
    }

    private void PlayerStateController_OnStateChange(PlayerState arg1, PlayerState arg2)
    {
        if (arg2 == PlayerState.IDLE)
        {
            StartCoroutine(_ActivateProgressAfter(0.1f));
        }
    }

    private void OnDisable()
    {
        PlayerStateController.instance.OnPlayerTakeoverTower -= _OnPlayerTakeover;
        FireballGlovesController.OnFireballShoot -= _OnFireballThrow;
        if (dummyParent)
            dummyParent.SetActive(false);
    }
    void _OnPlayerTakeover(PlayerControllableTower tower)
    {
        
        _StopActivation();
        if (lastPanel != null)
            lastPanel.SetActive(false);
        StartCoroutine(_RecenterAfter());
        if (tower == null) {
            StartCoroutine(_ActivateProgressAfter(0.1f));
            return; }
        if (projectileSpawner != null)
            projectileSpawner.OnFire -= Spawner_OnFire;
        print($"Took over a {tower.dto.name}");
        if (tower.dto == archerDTO)
        {
            if (completed.Contains(archerDTO))
            {
                StartCoroutine(_ActivateProgressAfter(0.1f));
                return;
            }
            print($"We took over an archer!");
            archerPanel.SetActive(true);
            PullInteraction.PullActionReleased += _OnPullReleased;
            lastPanel = archerPanel;
        }

        else if (tower.dto == cannonDTO)
        {
            if (completed.Contains(cannonDTO))
            {
                StartCoroutine(_ActivateProgressAfter(0.1f));
                return;
            }
            print($"Took over a cannon!");
            cannonPanel.SetActive(true);
            lastPanel = cannonPanel;

            StartCoroutine(_checkSpawnerAfter());
        }
        else if (tower.dto == mageDTO)
        {
            if (completed.Contains(mageDTO))
            {
                StartCoroutine(_ActivateProgressAfter(0.1f));
                return;
            }
            magePanel.SetActive(true);
            lastPanel = magePanel;
        }
    }

    IEnumerator _checkSpawnerAfter()
    {
        yield return new WaitForSeconds(0.2f);
        ProjectileSpawner[] spawners = FindObjectsByType<ProjectileSpawner>(FindObjectsSortMode.None);

        foreach (var spawner in spawners)
        {
            if (spawner && spawner.gameObject.name.StartsWith("1_Hand Cannon Base"))
            {
                print($"Added listener to spawner {spawner.gameObject.name}");
                spawner.OnFire += Spawner_OnFire;
                projectileSpawner = spawner;
            }
        }
        print($"Checked {spawners.Count()} proj spawners, found it? {projectileSpawner != null}");
    }
    IEnumerator _RecenterAfter(float time = .3f)
    {
        yield return new WaitForSeconds(time);
        TutorialManager.RecenterGUI();

    }
    ProjectileSpawner projectileSpawner = null;
    int cannon_shots = 0;
    private void Spawner_OnFire()
    {
        if (completed.Contains(cannonDTO)) return;
        cannon_shots++;
        cannonCountText.text = $"{cannon_shots} / {CannonShotsRequired}";
        if (cannon_shots >= CannonShotsRequired)
        {
            cannonCountText.color = activeColor;
            completed.Add(cannonDTO);
            _ActivateProgress();
        }
        print($"pew!");
    }

    HashSet<Tower_SO> completed = new HashSet<Tower_SO> ();
    void _UpdateProgressText()
    {
        string s = "";
        Tower_SO[] towers = new Tower_SO[3] {archerDTO,  cannonDTO, mageDTO};
        foreach (var dto in towers)
        {
            var c = (completed.Contains(dto)) ? activeColor : inactiveColor;
            string color = ColorUtility.ToHtmlStringRGBA(c);
            s += $"- <color=#{color}>{dto.name}</color>\n";

        }
        s = s.Trim();
        progressText.text = s;
    }
    int pulls = 0;
    void _OnPullReleased(float pull, TowerPlayerWeapon weapon = null)
    {
        pulls++;
        if (pulls >= ArcherShotsRequired)
        { 
            PullInteraction.PullActionReleased -= _OnPullReleased;
            archerCount.color = activeColor;
            completed.Add(archerDTO);
            //archerPanel.SetActive(false);
            _ActivateProgress();
        }

        archerCount.text = $"{pulls} / {ArcherShotsRequired}";
    }
    void _ActivateProgress(float time = 1.5f)
    {
        if (_currentActivator != null)
            StopCoroutine(_currentActivator);
        _currentActivator = _ActivateProgressAfter(time);
        StartCoroutine(_currentActivator);
    }
    void _StopActivation()
    {
        if (_currentActivator != null)
            StopCoroutine(_currentActivator);
        _currentActivator = null;
    }
    IEnumerator _currentActivator;
    IEnumerator _ActivateProgressAfter(float time = 1.5f)
    {
        yield return new WaitForSeconds(time);
        if (lastPanel != null)
            lastPanel.SetActive(false);
        lastPanel = progressPanel;
        progressPanel.SetActive(true);
        _UpdateProgressText();
        _currentActivator = null;
        if (completed.Count == 3)
        {
            print($"Done with towers, skipping");
            yield return new WaitForSeconds(3f);
            PlayerStateController.OnStateChange -= PlayerStateController_OnStateChange;
            TutorialManager.SetSkip();
        }
    }

    int fireballsThrown = 0;
    void _OnFireballThrow()
    {
        if (completed.Contains(mageDTO)) return;
        fireballsThrown++;
        lastPanel = magePanel;
        if (fireballsThrown >= mageShotsRequired)
        {
            print($"Fireballs completed, updating progress");
            completed.Add(mageDTO);
            mageCountText.color = activeColor;
            _ActivateProgress();
        }
        mageCountText.text = $"{fireballsThrown} / {mageShotsRequired}";
    }
}
