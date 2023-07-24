using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RadialTargetingSystem : MonoBehaviour
{
    private readonly List<Enemy> _targetsInRange = new();
    [SerializeField] private SphereCollider _collider;

    private void Awake()
    {
        if(_collider == null)
            _collider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Enemy e))
        {
            _targetsInRange.Add(e);
            e.healthController.OnDeath += OnTargetDeath;
        }
    }

    private void OnTargetDeath()
    {
        _targetsInRange.RemoveAll(t=> t == null);
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent(out Enemy e))
        {
            _targetsInRange.Remove(e);
            e.healthController.OnDeath -= OnTargetDeath;
        }    
    }

    public Enemy GetClosestTarget(Vector3 pos)
    {
        return _targetsInRange.OrderBy(t => Vector3.Distance(t.transform.position, pos)).FirstOrDefault();
    }    
    
    public Enemy GetOldestTarget()
    {
        return _targetsInRange.OrderBy(t => t.spawnTime).FirstOrDefault();
    }

    public bool HasTarget() => _targetsInRange.Any(t => t != null);

    public void SetRadius(float newRadius)
    {
        _collider.radius = newRadius;
    }
}