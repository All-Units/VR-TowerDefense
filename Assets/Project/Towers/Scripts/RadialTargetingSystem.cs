using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RadialTargetingSystem : MonoBehaviour
{
    public List<Enemy> _targetsInRange = new();
    [SerializeField] private SphereCollider _collider;

    public Action<Enemy> OnEnter;
    public Action<Enemy> OnExit;

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
        if(other.TryGetComponent(out Enemy e) && _targetsInRange.Contains(e) == false)
        {
            _targetsInRange.Add(e);
            OnEnter?.Invoke(e);
        }
    }

    private void OnTargetDeath(Enemy e)
    {
        _targetsInRange.Remove(e);
        OnExit?.Invoke(e);
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent(out Enemy e) && _targetsInRange.Contains(e))
        { 
            _targetsInRange.Remove(e);
            OnExit?.Invoke(e);
        }
        
    }
    const float RecheckRate = 0.5f;
    float _lastCheckTime = 0f;
    Enemy _lastClosestEnemy = null;
    public Enemy GetClosestTarget(Vector3 pos)
    {
        //If we've check recently, return our cached value
        if (Time.time - _lastCheckTime <= RecheckRate && _lastClosestEnemy != null)
        {
            return _lastClosestEnemy;
        }
        _lastCheckTime = Time.time;
        _CullTargets();
        //Reorder targets by distance
        _targetsInRange = _targetsInRange.OrderBy(t => Utilities.FlatDistance(t.transform.position, pos)).ToList();
        //_targetsInRange = _targetsInRange.OrderBy(t => t.transform.position.y).ToList();
        _lastClosestEnemy = _targetsInRange.FirstOrDefault();
        return _lastClosestEnemy;
    }    
    
    public Enemy GetOldestTarget()
    {
        _CullTargets();
        
        return _targetsInRange.OrderBy(t => t.spawnTime).FirstOrDefault();
    }
    public HashSet<Enemy> _blacklist = new HashSet<Enemy> { };  
    void _CullTargets()
    {
        _targetsInRange.RemoveAll(e => !e);
        _targetsInRange.RemoveAll(e => _blacklist.Contains(e));
        _targetsInRange.RemoveAll(e => e.gameObject.activeInHierarchy == false);

        //If there is any living target, cull all the dead
        //Otherwise, have at 'em
        if (_targetsInRange.Exists(e => e.healthController.isDead == false))
            _targetsInRange.RemoveAll(e => e.healthController.isDead);
    }
    public bool HasTarget()
    {
        _CullTargets();
        return _targetsInRange.Any(t => t != null);
    } 

    public void SetRadius(float newRadius)
    {
        _collider.radius = newRadius;
    }
}