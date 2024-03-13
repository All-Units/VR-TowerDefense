using System.Collections;
using System.Linq;
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
    [SerializeField] private float reloadTime = 2f;
    private int currentAmmo;

    private ProjectileTower_SO projectileTowerSo => dto as ProjectileTower_SO;

    public float ProjectileSpeed => projectileTowerSo.projectile.speed;

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
        if (XRPauseMenu.IsPaused) return;
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
        
        if(ammo > 0 && currentAmmo <= 0)
            return;
        
        if (_currentCooldown <= 0)
        {
            if (!targetingSystem.HasTarget()) return;
            StartCoroutine(_FireAfterDelay());
            
            //Fire();
        }
        else
        {
            _currentCooldown -= Time.deltaTime;
        }
    }
    bool _isFiring = false;
    IEnumerator _FireAfterDelay()
    {
        if (_isFiring) yield break;
        _isFiring = true;
        yield return new WaitForSeconds(0.2f);
        Fire();
        _isFiring = false;
    }
    public Transform GetCurrentTarget => 
        (targetingSystem.HasTarget()) ? targetingSystem.GetClosestTarget(transform.position).transform : null;
    public float GetHeightOffset => enemyHeightOffset;
    private void AimAtTarget()
    {
        return;
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
            Enemy target = targetingSystem._targetsInRange.FirstOrDefault();
            if (_currentMissileSelector == null)
            {
                _currentMissileSelector = _SelectMissileTarget();
                StartCoroutine(_currentMissileSelector);
            }
            else
                target = targetingSystem._targetsInRange.GetRandom();
            missileController.SetTarget(target);
            missileController.HitTarget();
        }

        if (projectile.TryGetComponent(out MissileController component))
        {
            component.SetTarget(targetingSystem.GetOldestTarget());
        }
        
        if (ammo > 0)
        {
            currentAmmo--;
        }
            
        foreach (var auxFirePoint in auxFirePoints)
        {
            if (ammo > 0 && currentAmmo <= 0) return;
            
            var auxProjectile = Instantiate(projectileTowerSo.projectile, auxFirePoint.position, auxFirePoint.rotation);
            auxProjectile.Fire();
            
            if (ammo > 0)
            {
                currentAmmo--;
            }
        }

        if (ammo > 0 && currentAmmo <= 0)
            ReloadRoutine ??= StartCoroutine(Reload());
        
        _currentCooldown = projectileTowerSo.shotCooldown;
    }
    IEnumerator _currentMissileSelector = null;
    IEnumerator _SelectMissileTarget()
    {
        yield return new WaitForSeconds(0.1f);
        _currentMissileSelector = null;
    }

    private Coroutine ReloadRoutine;

    private IEnumerator Reload()
    { 
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = ammo;
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