﻿using System;
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

    public Enemy GetClosestTarget(Vector3 pos)
    {
        return _targetsInRange.OrderBy(t => Vector3.Distance(t.transform.position, pos)).FirstOrDefault();
    }    
    
    public Enemy GetOldestTarget()
    {
        _CullTargets();
        
        return _targetsInRange.OrderBy(t => t.spawnTime).FirstOrDefault();
    }
    void _CullTargets()
    {
        _targetsInRange.RemoveAll(e => !e);

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