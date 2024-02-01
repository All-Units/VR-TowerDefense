using System.Collections;
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

    [SerializeField] private int ammo = -1;
    private int currentAmmo;

    private ProjectileTower_SO projectileTowerSo => dto as ProjectileTower_SO;

    protected override void Awake()
    {
        base.Awake();
        targetingSystem.SetRadius(projectileTowerSo.radius);
        if(attackVFX)
            attackVFX.SetActive(false);

        if (ammo > 0)
            currentAmmo = ammo;
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
        
        if(ammo > 0 && currentAmmo <= 1)
            return;
        
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
            
            missileController.SetTarget(targetingSystem._targetsInRange.GetRandom());
            missileController.HitTarget();
            if (ammo > 0)
            {
                currentAmmo--;
            }
        }

        foreach (var auxFirePoint in auxFirePoints)
        {
            var auxProjectile = Instantiate(projectileTowerSo.projectile, auxFirePoint.position, auxFirePoint.rotation);
            auxProjectile.Fire();
            if (ammo > 0)
            {
                currentAmmo--;
            }
        }

        ReloadRoutine ??= StartCoroutine(Reload());
        
        _currentCooldown = projectileTowerSo.shotCooldown;
    }

    private Coroutine ReloadRoutine;

    private IEnumerator Reload()
    {
        while (currentAmmo < ammo)
        {
            yield return new WaitForSeconds(.35f);
            currentAmmo++;
        }

        ReloadRoutine = null;
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