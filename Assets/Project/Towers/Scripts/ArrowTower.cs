using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

public enum PlayerItemType
{
    Bow,
    Staff,
    Cannon
}
public class ArrowTower : Tower
{
    [SerializeField] private float radius = 5f;
    [SerializeField] private RadialTargetingSystem targetingSystem;

    [SerializeField] private float shotCooldown = 1f;
    private float _currentCooldown;
    [SerializeField] private Projectile projectile;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform pivotPoint;

    [FormerlySerializedAs("vfx")] [Header("VFX")] 
    [SerializeField] private GameObject attackVFX;
    [FormerlySerializedAs("SelectedVfx")] [SerializeField] private GameObject selectedVfx;
    [SerializeField] private GameObject turretModel;
    [SerializeField] private GameObject playerPlatform;

    [SerializeField] private PlayerItemType playerItemType;
    
    protected override void Awake()
    {
        base.Awake();
        targetingSystem.SetRadius(radius);
        selectedVfx.SetActive(false);
    }

    private void Update()
    {
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
        var go = Instantiate(projectile, firePoint.position, firePoint.rotation);
        _currentCooldown = shotCooldown;
    }

    public override void Selected()
    {
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

        InventoryManager.instance.GivePlayerItem(playerItemType);
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
        Gizmos.DrawWireSphere(targetingSystem.transform.position, radius);
        
        Gizmos.color = targetingSystem.HasTarget() ? Color.red : Color.blue;

        Gizmos.DrawLine(firePoint.position, firePoint.forward + firePoint.position);
    }

    #endregion
}