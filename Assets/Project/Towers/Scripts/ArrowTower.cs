using UnityEngine;
using UnityEngine.Serialization;

public class ArrowTower : Tower
{
    [SerializeField] private TowerStats stats;
    
    [SerializeField] private RadialTargetingSystem targetingSystem;

    private float _currentCooldown;

    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform pivotPoint;

    [FormerlySerializedAs("vfx")] [Header("VFX")] 
    [SerializeField] private GameObject attackVFX;
    [FormerlySerializedAs("SelectedVfx")] [SerializeField] private GameObject selectedVfx;
    [SerializeField] private GameObject turretModel;
    [SerializeField] private GameObject playerPlatform;

    [SerializeField] private PlayerItem_SO playerItemSO;
    protected override void Awake()
    {
        base.Awake();
        targetingSystem.SetRadius(stats.radius);
        selectedVfx.SetActive(false);
        if(attackVFX)
            attackVFX.SetActive(false);
    }

    private void Update()
    {
        if(!isInitialized) return; 
        
        if(isPlayerControlled) return;
        
        if(targetingSystem.HasTarget())
        {
            AimAtTarget();
            if (attackVFX)
            {
                if(attackVFX.activeSelf == false)
                    attackVFX.SetActive(true);
            }
        }
        else
        {
            if (attackVFX)
            {
                if (attackVFX.activeSelf)
                    attackVFX.SetActive(false);
            }
        }
        
        if (_currentCooldown <= 0)
        {
            if (!targetingSystem.HasTarget()) return;
            
            Fire();
        }
        else
        {
            _currentCooldown -= Time.deltaTime;
        }
    }

    private void AimAtTarget()
    {
        var oldestTarget = targetingSystem.GetOldestTarget();
        if(oldestTarget)
            pivotPoint.LookAt(oldestTarget.transform);
    }

    private void Fire()
    {
        var projectile = Instantiate(stats.projectile, firePoint.position, firePoint.rotation);
        projectile.Fire();
        _currentCooldown = stats.shotCooldown;
    }

    public override void Selected()
    {
        base.Selected();
        selectedVfx.SetActive(true);
    }

    public override void Deselected()
    {
        selectedVfx.SetActive(false);
    }

    public override void PlayerTakeControl()
    {
        base.PlayerTakeControl();
        
        // Hide turret
        turretModel.SetActive(false);
        playerPlatform.SetActive(true);

        InventoryManager.instance.GivePlayerItem(playerItemSO);
    }

    public override void PlayerReleaseControl()
    {
        base.PlayerReleaseControl();
        
        // Show turret
        turretModel.SetActive(true);
        playerPlatform.SetActive(false);
        
        InventoryManager.instance.ReleaseAllItems();
    }

    #region Debugging

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetingSystem.transform.position, stats.radius);
        
        Gizmos.color = targetingSystem.HasTarget() ? Color.red : Color.blue;

        Gizmos.DrawLine(firePoint.position, firePoint.forward + firePoint.position);
    }

    #endregion
}

