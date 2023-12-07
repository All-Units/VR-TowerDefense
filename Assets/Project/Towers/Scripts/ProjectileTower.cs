using UnityEngine;
using UnityEngine.Serialization;

public class ProjectileTower : Tower
{
    [Header("Projectile Tower")] 
    [SerializeField] private RadialTargetingSystem targetingSystem;

    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform pivotPoint;

    [Header("VFX")] 
    [SerializeField] private GameObject attackVFX;
    [FormerlySerializedAs("SelectedVfx")] 
    [SerializeField] private GameObject turretModel;
    [SerializeField] private GameObject playerPlatform;
        private float _currentCooldown;

    protected override void Awake()
    {
        base.Awake();
        targetingSystem.SetRadius(dto.stats.radius);
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
        var target = oldestTarget.transform.position;
        target += Vector3.up;
        if(oldestTarget)
            pivotPoint.LookAt(target);
    }

    private void Fire()
    {
        var projectile = Instantiate(dto.stats.projectile, firePoint.position, firePoint.rotation);
        projectile.Fire();
        _currentCooldown = dto.stats.shotCooldown;
    }

    public override void PlayerTakeControl()
    {
        base.PlayerTakeControl();
        
        // Hide turret
        turretModel.SetActive(false);
        playerPlatform.SetActive(true);

        InventoryManager.instance.GivePlayerItem(dto.playerItem_SO);
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
        Gizmos.DrawWireSphere(targetingSystem.transform.position, dto.stats.radius);
        
        Gizmos.color = targetingSystem.HasTarget() ? Color.red : Color.blue;

        Gizmos.DrawLine(firePoint.position, firePoint.forward + firePoint.position);
    }

    #endregion
}

