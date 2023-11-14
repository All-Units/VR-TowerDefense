using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage;
    private Rigidbody rb;
    public float speed = 20f;

    protected bool isDestroying = false;

    public StatusModifier statusModifier;
    [SerializeField] protected AudioClipController _hitEnemy;
    [SerializeField] protected AudioClipController _hitGround;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * speed, ForceMode.VelocityChange);
        
        Destroy(gameObject, 10f);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"Trigger entered {other.gameObject.name}", other.gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (isDestroying) return;

        if (other.collider.isTrigger) return;
        
        OnCollision(other);
    }

    protected virtual void OnCollision(Collision other)
    {

        var colliderGameObject = other.collider.gameObject;
        Vector3 pos = transform.position;
        if (colliderGameObject.TryGetComponent(out HealthController healthController))
        {
            healthController.TakeDamage(damage);
            
            if(statusModifier)
            {
                var statusEffectController = healthController.GetComponentInChildren<StatusEffectController>();
                if(statusEffectController)
                    statusModifier.ApplyStatus(statusEffectController);
            }
            if (_hitEnemy)
                _hitEnemy.PlayClipAt(pos);
        }
        else
        {
            if(_hitGround)
                _hitGround.PlayClipAt(pos);
        }

        isDestroying = true;
        Destroy(gameObject);
    }
}