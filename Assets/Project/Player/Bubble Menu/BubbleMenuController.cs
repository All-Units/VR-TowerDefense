using System.Linq;
using Project.Towers.Scripts;
using UnityEngine;

public class BubbleMenuController : MonoBehaviour
{
    private static BubbleMenuController Instance;
    private Tower _currentTower;
    [SerializeField] private GameObject towerCamera;

    [SerializeField] private BubbleMenuOption upgradeOption;

    private void Awake()
    {
        Instance = this;
        _Hide();
        
        PlayerStateController.OnStateChange += PlayerStateControllerOnOnStateChange;
    }

    private void PlayerStateControllerOnOnStateChange(PlayerState arg1, PlayerState arg2)
    {
        if(arg2 == PlayerState.TOWER_CONTROL)
            _Hide();
    }

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

        var towerUpgrades = _currentTower.dto.GetUpgrades();
        if(towerUpgrades.Count > 0)
            upgradeOption.InitializeUpgrade(this, towerUpgrades[0]);
        else
        {
            upgradeOption.Disable();
        }
    }

    private void ListUpgrades()
    {
        var upgrades = _currentTower.dto.GetUpgrades();
    }

    public static void Open(Tower tower)
    {
        if(Instance == null) return;
        
        Instance.Initialize(tower);
    }

    public static void Hide()
    {
        if (Instance == null) return;
        
        Instance._Hide();
    }

    private void _Hide()
    {
        gameObject.SetActive(false);
    }

    public void Upgrade(TowerUpgrade towerUpgrade)
    {
        TowerSpawnManager.UpgradeTower(_currentTower, towerUpgrade.upgrade);
    }
}