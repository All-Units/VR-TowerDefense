using System;
using UnityEngine;

public class ArrowTower : Tower
{
    [SerializeField] private float radius = 5f;
    [SerializeField] private RadialTargetingSystem targetingSystem;

    [SerializeField] private float shotCooldown = 1f;
    private float _currentCooldown;
    [SerializeField] private Projectile projectile;
    [SerializeField] private Transform firePoint;

    protected override void Awake()
    {
        base.Awake();
        targetingSystem.SetRadius(radius);
    }

    private void Update()
    {
        if(targetingSystem.HasTarget())
            AimAtTarget();
        
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
            firePoint.LookAt(oldestTarget.transform);
    }

    private void Fire()
    {
        var go = Instantiate(projectile, firePoint.position, firePoint.rotation);
        _currentCooldown = shotCooldown;
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