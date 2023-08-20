using UnityEngine;

public class ArrowTower : Tower
{
    [SerializeField] private float radius = 5f;
    [SerializeField] private RadialTargetingSystem targetingSystem;

    [SerializeField] private float shotCooldown = 1f;
    private float _currentCooldown;
    [SerializeField] private Projectile projectile;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform pivotPoint;

    [Header("VFX")] [SerializeField] private GameObject vfx;
    [SerializeField] private GameObject SelectedVfx;
    protected override void Awake()
    {
        base.Awake();
        targetingSystem.SetRadius(radius);
        SelectedVfx.SetActive(false);
    }

    private void Update()
    {
        if(targetingSystem.HasTarget())
        {
            AimAtTarget();
            if (vfx)
            {
                if(vfx.activeSelf == false)
                    vfx.SetActive(true);
            }
        }
        else
        {
            if (vfx)
            {
                if (vfx.activeSelf)
                    vfx.SetActive(false);
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
        SelectedVfx.SetActive(true);
    }

    public override void Deselected()
    {
        SelectedVfx.SetActive(false);
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