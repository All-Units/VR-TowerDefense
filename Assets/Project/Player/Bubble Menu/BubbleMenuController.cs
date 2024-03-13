using System;
using Project.Towers.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

public class BubbleMenuController : MonoBehaviour
{
    private static BubbleMenuController _instance;
    private Tower _currentTower;
    public GameObject towerCamera;

    [SerializeField] private BubbleMenuOption upgradeOption1;
    [SerializeField] private BubbleMenuOption upgradeOption2;
    [SerializeField] private BubbleMenuOption sellOption;
    [SerializeField] private BubbleMenuOption takeoverOption;
    
    [SerializeField]
    [Tooltip("The reference to the action to confirm tower takeover selection.")]
    private InputActionReference closeMenuActionReference; 
    InputAction closeBubbleMenuAction => Utilities.GetInputAction(closeMenuActionReference);

    private void Awake()
    {
        _instance = this;
        _Hide();
        
        PlayerStateController.OnStateChange += PlayerStateControllerOnOnStateChange;
        CurrencyManager.OnChangeMoneyAmount += CurrencyManagerOnChangeMoneyAmount;
        
        if (closeBubbleMenuAction != null)
        {
            closeBubbleMenuAction.started += CloseTowerBubbles;
        }
        XRPauseMenu.OnPause += _Hide;
    }

    private void OnDestroy()
    {
        PlayerStateController.OnStateChange -= PlayerStateControllerOnOnStateChange;
        CurrencyManager.OnChangeMoneyAmount -= CurrencyManagerOnChangeMoneyAmount;
        XRPauseMenu.OnPause -= _Hide;
        if (closeBubbleMenuAction != null)
            closeBubbleMenuAction.started -= CloseTowerBubbles;

        _instance = null;
    }

    private void PlayerStateControllerOnOnStateChange(PlayerState arg1, PlayerState arg2)
    {
        if(arg2 == PlayerState.TOWER_CONTROL)
            _Hide();
    }

    #region Initialization

    private void Initialize(Tower t)
    {
        if(_currentTower)
            _currentTower.EndFocus();
        
        _currentTower = t;
        _currentTower.StartFocus();
        
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

        bool isTakeover = _currentTower is PlayerControllableTower;
        takeoverOption.gameObject.SetActive(isTakeover);
        
        if(isTakeover)
            takeoverOption.Initialize(TakeoverTower, "Takeover");
            
    }
    void _Lock(BubbleMenuOption option, bool unlocked)
    {
        Transform lockTransform = option.transform.Find("lock");
        if (lockTransform == null) return;
        bool active = unlocked == false;

        lockTransform.gameObject.SetActive(active);
    }
    private void ListUpgrades()
    {
        if(_currentTower == null) return;
        
        var towerUpgrades = _currentTower.dto.GetUpgrades();
        if(towerUpgrades.InRange(0))
        {
            bool unlocked = towerUpgrades[0].upgrade.IsUnlocked;
            _Lock(upgradeOption1, unlocked);
            upgradeOption1.Initialize(() => Upgrade(towerUpgrades[0]), towerUpgrades[0].upgrade.name, towerUpgrades[0].upgrade.cost);
        }       
        else
        {
            upgradeOption1.Hide();
        }         
        
        if(towerUpgrades.InRange(1))
        {
            bool unlocked = towerUpgrades[1].upgrade.IsUnlocked;
            _Lock(upgradeOption2, unlocked);
            upgradeOption2.Initialize(() => Upgrade(towerUpgrades[1]), towerUpgrades[1].upgrade.name, towerUpgrades[1].upgrade.cost);
        }   
        else
        {
            upgradeOption2.Hide();
        }
    }
    #endregion

    #region State Management

    public static void Open(Tower tower)
    {
        if(_instance == null) return;
        XRControllerTowerController._CloseBubbles();
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
        if(_currentTower)
            _currentTower.EndFocus();
        XRControllerTowerController.DeselectCurrent();
    }
    
    private void CloseTowerBubbles(InputAction.CallbackContext obj)
    {
        _Hide();
    }

    private void CurrencyManagerOnChangeMoneyAmount(int obj)
    {
        if(gameObject.activeInHierarchy)
            ListUpgrades();
    }

    #endregion


    #region Actions

    public void Upgrade(TowerUpgrade towerUpgrade)
    {
        if (towerUpgrade.upgrade.IsUnlocked == false) return;
        if (CurrencyManager.CanAfford(towerUpgrade.upgrade.cost) == false)
        {
            return;
        }
        
        
        CurrencyManager.TakeFromPlayer(towerUpgrade.upgrade.cost);
        
        //Debug.Log($"Upgrading: {_currentTower} to {towerUpgrade.upgrade.name}");
        var newTower = TowerSpawnManager.UpgradeTower(_currentTower, towerUpgrade.upgrade);
        Hide();
    }
    
    private void SellTower()
    {
        TowerSpawnManager.SellTower(_currentTower);
        Hide();
    }  
    
    private void TakeoverTower()
    {
        if(_currentTower is PlayerControllableTower playerControllableTower)
            PlayerStateController.TakeControlOfTower(playerControllableTower);

        Hide();
    }

    #endregion

}