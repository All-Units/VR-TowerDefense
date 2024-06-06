using Project.Towers.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class BubbleMenuController : MonoBehaviour
{
    private static BubbleMenuController _instance;
    private Tower _currentTower;
    public GameObject towerCamera;

    [SerializeField] private float distanceFromPlayer = 1;

    [SerializeField] private BubbleMenuOption upgradeOption1;
    [SerializeField] private BubbleMenuOption upgradeOption2;
    [SerializeField] private BubbleMenuOption sellOption;
    [SerializeField] private BubbleMenuOption takeoverOption;
    [SerializeField] private BubbleMenuOption repairOption;
    [SerializeField] private SliderController healthbarController;
    [SerializeField] private int repairCost;
    
    [SerializeField] [Tooltip("The reference to the action to confirm tower takeover selection.")]
    private InputActionReference closeMenuActionReference;
    private InputAction closeBubbleMenuAction => Utilities.GetInputAction(closeMenuActionReference);

    private void Awake()
    {
        _instance = this;
        _Hide();
        
        PlayerStateController.OnStateChange += PlayerStateControllerOnOnStateChange;
        CurrencyManager.OnChangeMoneyAmount += CurrencyManagerOnChangeMoneyAmount;
        Tower.OnTowerDestroy += TowerOnTowerDestroy;
        XRPauseMenu.OnPause += _Hide;
        
        if (closeBubbleMenuAction != null)
            closeBubbleMenuAction.started += CloseTowerBubbles;
    }



    private void OnDestroy()
    {
        PlayerStateController.OnStateChange -= PlayerStateControllerOnOnStateChange;
        CurrencyManager.OnChangeMoneyAmount -= CurrencyManagerOnChangeMoneyAmount;
        Tower.OnTowerDestroy += TowerOnTowerDestroy;
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
        {
            _currentTower.EndFocus();
            _currentTower.healthController.onTakeDamage.RemoveListener(healthbarController.UpdateValue);
        }
        
        _currentTower = t;
        _currentTower.StartFocus();
        
        towerCamera.transform.position = t.transform.position;
        towerCamera.gameObject.SetActive(true);
        gameObject.SetActive(true);
        
        var main = Camera.main;
        if (main != null)
        {
            var camTransform = main.transform;
            var camPosition = camTransform.position;
            
            transform.position = camPosition + (camTransform.forward * distanceFromPlayer);
            transform.LookAt(camPosition);
        }

        ListUpgrades();
        sellOption.Initialize(SellTower, $"Sell: ${_currentTower.dto.cost/2}");

        var isTakeover = _currentTower is PlayerControllableTower;
        takeoverOption.gameObject.SetActive(isTakeover);
        
        if(isTakeover)
            takeoverOption.Initialize(TakeoverTower, "Takeover");
            
        repairOption.Initialize(RepairTower, $"Repair: ${repairCost}", repairCost);
        healthbarController.SetBounds(_currentTower.healthController.MaxHealth);
        healthbarController.UpdateValue(_currentTower.healthController.CurrentHealth);
        _currentTower.healthController.onTakeDamage.AddListener(healthbarController.UpdateValue);
    }

    private void RepairTower()
    {
        if(_currentTower.canRepair == false) return;
        if (CurrencyManager.CanAfford(repairCost) == false)
        {
            return;
        }
        
        CurrencyManager.TakeFromPlayer(repairCost);
        _currentTower.Repair();
    }

    private static void _Lock(BubbleMenuOption option, bool unlocked)
    {
        var lockTransform = option.transform.Find("lock");
        if (lockTransform == null) return;
        var active = unlocked == false;
        lockTransform.gameObject.SetActive(active);
        XRSimpleInteractable simple = option.GetComponent<XRSimpleInteractable>();
        if (simple == null) return;

        simple.firstHoverEntered.RemoveListener(_OnLockHovered);
        simple.lastHoverExited.RemoveListener(_OnLockEndHover);

        if (unlocked) return;
        //simple.firstHoverEntered.RemoveAllListeners();
        //simple.lastHoverExited.RemoveAllListeners();

        simple.firstHoverEntered.AddListener(_OnLockHovered);
        simple.lastHoverExited.AddListener(_OnLockEndHover);
    }
    static void _OnLockHovered(HoverEnterEventArgs a)
    {
        var t = a.interactableObject.transform.Find("lockText");
        if (t == null) return;
        t.gameObject.SetActive(true);
        t = a.interactableObject.transform.Find("DescriptionText");
        if (t == null) return;
        t.GetComponent<TMP_Text>().text = "";
    }
    static void _OnLockEndHover(HoverExitEventArgs a)
    {
        var t = a.interactableObject.transform.Find("lockText");
        if (t == null) return;
        t.gameObject.SetActive(false);
    }
    
    private void ListUpgrades()
    {
        if(_currentTower == null) return;
        
        var towerUpgrades = _currentTower.dto.GetUpgrades();
        TowerUpgrade upgrade;
        if(towerUpgrades.InRange(0))
        {
            var unlocked = towerUpgrades[0].upgrade.IsUnlocked;
            
            upgrade = towerUpgrades[0];
            upgradeOption1.Initialize(() => Upgrade(upgrade), upgrade.upgrade.name, upgrade.upgrade.cost, upgrade.upgrade.description);
            _Lock(upgradeOption1, unlocked);
            upgradeOption1._upgradeDTO = upgrade.upgrade;
        }       
        else
        {
            upgradeOption1.Hide();
        }         
        
        if(towerUpgrades.InRange(1))
        {
            var unlocked = towerUpgrades[1].upgrade.IsUnlocked;

            upgrade = towerUpgrades[1];
            upgradeOption2.Initialize(() => Upgrade(upgrade), upgrade.upgrade.name, upgrade.upgrade.cost, upgrade.upgrade.description);
            _Lock(upgradeOption2, unlocked);
            upgradeOption2._upgradeDTO = upgrade.upgrade;
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
        if (gameObject == null) return;
        try
        {
            gameObject.SetActive(false);
        }
        catch (MissingReferenceException e) { return; }
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

    private void TowerOnTowerDestroy(Tower obj)
    {
        if(_currentTower == obj && _instance)
            _Hide();
    }

    #endregion


    #region Actions
    public static void Upgrade(Tower_SO dto)
    {
        if (_instance == null) return;
        TowerUpgrade upgrade = new TowerUpgrade(dto);
        _instance.Upgrade(upgrade);
    }
    public void Upgrade(TowerUpgrade towerUpgrade)
    {
        if (towerUpgrade.upgrade.IsUnlocked == false) return;
        if (CurrencyManager.CanAfford(towerUpgrade.upgrade.cost) == false)
        {
            return;
        }
        CurrencyManager.TakeFromPlayer(towerUpgrade.upgrade.cost);
        
        TowerSpawnManager.UpgradeTower(_currentTower, towerUpgrade.upgrade);
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