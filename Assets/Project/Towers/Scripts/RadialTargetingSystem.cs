using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RadialTargetingSystem : MonoBehaviour
{
    public List<Enemy> _targetsInRange = new();
    [SerializeField] private SphereCollider _collider;

    private void Awake()
    {
        if(_collider == null)
            _collider = GetComponent<SphereCollider>();
        
        Enemy.OnDeath += OnTargetDeath;
    }

    private void OnDestroy()
    {
        Enemy.OnDeath -= OnTargetDeath;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Enemy e))
        {
            _targetsInRange.Add(e);
        }
    }

    private void OnTargetDeath(Enemy e)
    {
        _targetsInRange.Remove(e);
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent(out Enemy e))
        {
            _targetsInRange.Remove(e);
        }    
    }

    public Enemy GetClosestTarget(Vector3 pos)
    {
        return _targetsInRange.OrderBy(t => Vector3.Distance(t.transform.position, pos)).FirstOrDefault();
    }    
    
    public Enemy GetOldestTarget()
    {
        _targetsInRange.RemoveAll(e => !e);
        return _targetsInRange.OrderBy(t => t.spawnTime).FirstOrDefault();
    }

    public bool HasTarget() => _targetsInRange.Any(t => t != null);

    public void SetRadius(float newRadius)
    {
        _collider.radius = newRadius;
    }
}