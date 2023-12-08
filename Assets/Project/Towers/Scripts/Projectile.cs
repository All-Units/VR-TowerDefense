using System;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.PlayerSettings;

public class Projectile : MonoBehaviour
{
    public int damage;
    private Rigidbody rb;
    public float speed = 20f;

    protected bool isDestroying = false;

    public StatusModifier statusModifier;
    
    [SerializeField] protected AudioClipController _hitEnemy;
    [SerializeField] protected AudioClipController _hitGround;

    [SerializeField] private GameObject flyingVFX;

    public UnityEvent OnFire;
    public UnityEvent OnHit;
    Vector3 startPos;
    public void Fire()
    {
        transform.SetParent(null);
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.AddForce(transform.forward * speed, ForceMode.VelocityChange);

        if(flyingVFX)
            flyingVFX.SetActive(true);
        Destroy(gameObject, 20f);
        startPos = transform.position;
        OnFire?.Invoke();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (isDestroying) return;

        if (other.collider.isTrigger) return;
        
        OnCollision(other.collider);
    }

    protected virtual void OnCollision(Collider other)
    {

        var colliderGameObject = other.gameObject;
        if (colliderGameObject.TryGetComponent(out HealthController healthController))
        {
            healthController.TakeDamage(damage);

            ApplyEffects(healthController);
            
            // Todo Refactor out to event based
            if (_hitEnemy)
                _hitEnemy.PlayClipAt(transform.position);
        }
        else
        {
            // Todo Refactor out to event based
            if (_hitGround)
                _hitGround.PlayClipAt(transform.position);
        }
        BasicEnemy.FlingRagdoll(colliderGameObject, startPos);

        OnHit?.Invoke();
        isDestroying = true;
        Destroy(gameObject);
    }

    protected void ApplyEffects(HealthController healthController)
    {
        if (statusModifier)
        {
            var statusEffectController = healthController.GetComponentInChildren<StatusEffectController>();
            if (statusEffectController)
                statusModifier.ApplyStatus(statusEffectController);
        }
    }
}