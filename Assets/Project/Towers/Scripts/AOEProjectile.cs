﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class AOEProjectile : Projectile
{
    public float splashRadius = 5;
    public GameObject hitParticles;

    [SerializeField] private AnimationCurve damageDropOff;

    bool validDestroy = false;
    
    

    protected override void OnDestroy()
    {
        base.OnDestroy();
        bool wasLastFrameResume = Time.frameCount == lastFrameResumed + 1;
        if (validDestroy == false)
        {
            if (wasLastFrameResume)
                _CopySelf();
        }
    }
    /// <summary>
    /// Duplicates an AoE projectile if it was destroyed by OnResume
    /// </summary>
    void _CopySelf()
    {
        GameObject copy = Instantiate(gameObject);
        copy.transform.position = transform.position;
        copy.transform.rotation = transform.rotation;
        copy.SetActive(true);
        var copy_rb = copy.GetComponent<Rigidbody>();
        var rb = GetComponent<Rigidbody>();
        copy_rb.velocity = rb.velocity;
        copy_rb.constraints = rb.constraints;
    }
    public string TargetLayer = "Enemy";
    protected override void OnCollision(Collider other)
    {
        
        Vector3 pos = transform.position;
        var hits = Physics.OverlapSphere(pos, splashRadius, LayerMask.GetMask(TargetLayer, "Ragdoll"));
        HashSet<HealthController> hcs = new HashSet<HealthController>();
        float _DestroyDelay = 0f;
        foreach (var hit in hits)
        {
            //Don't care about triggers
            if (hit.isTrigger) continue;

            var colliderGameObject = hit.gameObject;
            HealthController healthController = colliderGameObject.GetComponentInParent<HealthController>();
            //Only affect each HC once
            if (healthController && hcs.Contains(healthController) == false)
            {
                hcs.Add(healthController);
                var distance = Vector3.Distance(hit.ClosestPoint(pos), pos);
                var radius = distance/splashRadius;
                var dmg = Mathf.FloorToInt(damage * damageDropOff.Evaluate(Mathf.Clamp01(radius)));
                ApplyDamage(healthController, dmg, pos);
                StatusEffectController controller = healthController.GetComponentInChildren<StatusEffectController>();
                if (controller != null)
                {
                    _DestroyDelay = Math.Max(controller.EffectDuration, _DestroyDelay);
                }
                ApplyEffects(healthController);
            }
            BasicEnemy.FlingRagdoll(colliderGameObject, pos);
        }
        if(hitParticles)
        {
            var particles = Instantiate(hitParticles, pos, Quaternion.identity);
            particles.SetActive(true);
            particles.DestroyAfter(2f);
            //Destroy(particles, 2f);
        }
        //Turn obj into empty holder of Projectile
        if (_DestroyDelay != 0)
        {
            transform.position = new Vector3(0f, -8000f, 0f);
            //Go through reversed first, to check if dependencies?
            foreach (var comp in GetComponentsInChildren<Component>().Reverse())
            {
                //Early out
                if (comp is Transform) continue;
                if (comp == this) continue;
                if (comp is TowerPlayerWeapon) continue;
                try
                {
                    Destroy(comp);
                }
                catch { }
                
            }
            foreach (var comp in GetComponentsInChildren<Component>())
            {
                //Early out
                if (comp is Transform) continue;
                if (comp == this) continue;
                if (comp is TowerPlayerWeapon) continue;

                try
                {
                    Destroy(comp);
                }
                catch { }
            }

        }
        validDestroy = true;
        OnHit?.Invoke();
        isDestroying = true;
        Destroy(gameObject, _DestroyDelay);
    }

    public void ManualExplode()
    {
        Vector3 pos = transform.position;
        var hits = Physics.OverlapSphere(pos, splashRadius, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
        {
            var colliderGameObject = hit.gameObject;

            if (colliderGameObject.TryGetComponent(out HealthController healthController))
            {
                var distance = Vector3.Distance(hit.ClosestPoint(pos), pos);
                var radius = distance/splashRadius;
                int dmg = Mathf.FloorToInt(damage * damageDropOff.Evaluate(Mathf.Clamp01(radius)));
                
                ApplyDamage(healthController, dmg, pos);
                ApplyEffects(healthController);
            }
            
            BasicEnemy.FlingRagdoll(colliderGameObject, pos);
        }
        if(hitParticles)
        {
            var particles = Instantiate(hitParticles, pos, Quaternion.identity);
            Destroy(particles, 2f);
        }
        validDestroy = true;
        OnHit?.Invoke();
        isDestroying = true;
        Destroy(gameObject);
    }
    
}