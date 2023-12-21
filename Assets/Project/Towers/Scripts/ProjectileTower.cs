using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class ProjectileTower : PlayerControllableTower
{
    [Header("Projectile Tower")] 
    [SerializeField] private RadialTargetingSystem targetingSystem;

    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform pivotPoint;
    [SerializeField] private float EnemyHeightOffset = 2f;

    [Header("VFX")] 
    [SerializeField] private GameObject attackVFX;
    [FormerlySerializedAs("SelectedVfx")] 
    [SerializeField] private GameObject turretModel;
    [SerializeField] private GameObject playerPlatform;
    private float _currentCooldown;

    public UnityEvent onTakeover;
    public UnityEvent onRelease;    

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
        target += Vector3.up * EnemyHeightOffset;
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
        
        switch (dto.playerItem_SO)
        {
            case PlayerItem_SO itemSo:
                InventoryManager.instance.GivePlayerItem(itemSo);
                break;
            case PlayerPower power:
                InventoryManager.instance.GivePlayerPower(power);
                break;
        }
        
        onTakeover?.Invoke();
    }

    public override void PlayerReleaseControl()
    {
        base.PlayerReleaseControl();
        
        // Show turret
        turretModel.SetActive(true);
        playerPlatform.SetActive(false);
        
        InventoryManager.instance.ReleaseAllItems();
        onRelease?.Invoke();
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

