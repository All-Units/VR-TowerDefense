using System;
using Project.Towers.Scripts;
using UnityEngine;

public class BubbleMenuController : MonoBehaviour
{
    private static BubbleMenuController _instance;
    private Tower _currentTower;
    public GameObject towerCamera;

    [SerializeField] private BubbleMenuOption upgradeOption1;
    [SerializeField] private BubbleMenuOption upgradeOption2;
    [SerializeField] private BubbleMenuOption sellOption;

    private void Awake()
    {
        _instance = this;
        _Hide();
        
        PlayerStateController.OnStateChange += PlayerStateControllerOnOnStateChange;
    }

    private void OnDestroy()
    {
        PlayerStateController.OnStateChange -= PlayerStateControllerOnOnStateChange;
    }

    private void PlayerStateControllerOnOnStateChange(PlayerState arg1, PlayerState arg2)
    {
        if(arg2 == PlayerState.TOWER_CONTROL)
            _Hide();
    }

    #region Initialization

    private void Initialize(Tower t)
    {
        _currentTower = t;

        towerCamera.transform.position = t.transform.position;
        towerCamera.gameObject.SetActive(true);
        gameObject.SetActive(true);
        
        var main = Camera.main;
        if (main != null)
        {
            transform.position = main.transform.position + main.transform.forward;
            transform.LookAt(main.transform.position);
        }

        ListUpgrades();
        sellOption.Initialize(SellTower, $"Sell: ${_currentTower.dto.cost/2}");
    }

    private void ListUpgrades()
    {
        var towerUpgrades = _currentTower.dto.GetUpgrades();
        if(towerUpgrades.InRange(0))
        {
            upgradeOption1.Initialize(() => Upgrade(towerUpgrades[0]), towerUpgrades[0].upgrade.name);
        }       
        else
        {
            upgradeOption1.Disable();
        }         
        
        if(towerUpgrades.InRange(1))
        {
            upgradeOption2.Initialize(() => Upgrade(towerUpgrades[1]), towerUpgrades[1].upgrade.name);
        }   
        else
        {
            upgradeOption2.Disable();
        }
    }
    #endregion

    #region State Management

    public static void Open(Tower tower)
    {
        if(_instance == null) return;
        
        _instance.Initialize(tower);
    }

    public static void Hide()
    {
        if (_instance == null) return;
        
        _instance._Hide();
    }

    public void _Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion


    #region Actions

    public void Upgrade(TowerUpgrade towerUpgrade)
    {
        Debug.Log($"Upgrading: {_currentTower} to {towerUpgrade.upgrade.name}");
        var newTower = TowerSpawnManager.UpgradeTower(_currentTower, towerUpgrade.upgrade);
        // Open(newTower);
        Hide();
    }
    
    private void SellTower()
    {
        TowerSpawnManager.SellTower(_currentTower);
    }

    #endregion

}