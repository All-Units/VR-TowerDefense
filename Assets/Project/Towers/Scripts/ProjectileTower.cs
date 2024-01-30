using UnityEngine;
using UnityEngine.Serialization;

public class ProjectileTower : PlayerControllableTower
{
    [Header("Projectile Tower")] 
    [SerializeField] protected RadialTargetingSystem targetingSystem;

    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform[] auxFirePoints;
    [SerializeField] private Transform pivotPoint;
    [FormerlySerializedAs("EnemyHeightOffset")] [SerializeField] private float enemyHeightOffset = 2f;

    [Header("VFX")] 
    [SerializeField] private GameObject attackVFX;
    [FormerlySerializedAs("SelectedVfx")] 
    [SerializeField] private GameObject turretModel;
    [SerializeField] private GameObject playerPlatform;
    private float _currentCooldown;


    private ProjectileTower_SO projectileTowerSo => dto as ProjectileTower_SO;

    protected override void Awake()
    {
        base.Awake();
        targetingSystem.SetRadius(projectileTowerSo.radius);
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
        target += Vector3.up * enemyHeightOffset;
        if(oldestTarget)
            pivotPoint.LookAt(target);
    }

    private void Fire()
    {
        var projectile = Instantiate(projectileTowerSo.projectile, firePoint.position, firePoint.rotation);
        projectile.Fire();
        if (projectile.TryGetComponent(out GuidedMissileController missileController))
        {
            missileController.SetTarget(targetingSystem.GetOldestTarget());
            missileController.HitTarget();
        }

        foreach (var auxFirePoint in auxFirePoints)
        {
            var auxProjectile = Instantiate(projectileTowerSo.projectile, auxFirePoint.position, auxFirePoint.rotation);
            auxProjectile.Fire();
        }
        
        _currentCooldown = projectileTowerSo.shotCooldown;
    }

    public override void PlayerTakeControl()
    {
        
        // Hide turret
        turretModel.SetActive(false);
        playerPlatform.SetActive(true);
        
        switch (projectileTowerSo.playerItem_SO)
        {
            case PlayerItem_SO itemSo:
                InventoryManager.instance.GivePlayerItem(itemSo);
                break;
            case PlayerPower power:
                InventoryManager.instance.GivePlayerPower(power);
                break;
        }
        
        base.PlayerTakeControl();
    }

    public override void PlayerReleaseControl()
    {
        
        // Show turret
        turretModel.SetActive(true);
        playerPlatform.SetActive(false);
        
        InventoryManager.instance.ReleaseAllItems();
        
        base.PlayerReleaseControl();
    }

    #region Debugging

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetingSystem.transform.position, projectileTowerSo.radius);
        
        Gizmos.color = targetingSystem.HasTarget() ? Color.red : Color.blue;

        Gizmos.DrawLine(firePoint.position, firePoint.forward + firePoint.position);
    }

    #endregion
}