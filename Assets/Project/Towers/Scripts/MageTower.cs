using System.Collections.Generic;
using UnityEngine;

public class MageTower : PlayerControllableTower
{
    [SerializeField] private float radius = 5f;
    [SerializeField] private RadialTargetingSystem targetingSystem;

    [SerializeField] private float shotCooldown = 1f;
    private float _currentCooldown;
    [SerializeField] private AreaDamager damager;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform pivotPoint;

    protected override void Awake()
    {
        base.Awake();
        targetingSystem.SetRadius(radius);
    }

    private void Update()
    {
        if(targetingSystem.HasTarget() == false) return;
        
        AimAtTarget();
        damager.CleanUpTargets();
    }

    private void AimAtTarget()
    {
        var oldestTarget = targetingSystem.GetOldestTarget();
        if(oldestTarget)
            pivotPoint.LookAt(oldestTarget.transform);
    }

    private void Fire()
    {
        
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

public class AreaDamager : MonoBehaviour
{
    public List<HealthController> targets = new List<HealthController>();
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out HealthController healthController))
            targets.Add(healthController);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out HealthController healthController))
            targets.Remove(healthController);
    }

    public void CleanUpTargets()
    {
        targets.RemoveAll(t => t == null);
    }
}