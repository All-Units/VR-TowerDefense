using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage;
    private Rigidbody rb;
    public float speed = 20f;

    protected bool isDestroying = false;

    public StatusModifier statusModifier;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * speed, ForceMode.VelocityChange);
        
        Destroy(gameObject, 10f);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (isDestroying) return;

        OnCollision(other);
    }

    protected virtual void OnCollision(Collision other)
    {

        var colliderGameObject = other.collider.gameObject;

        if (colliderGameObject.TryGetComponent(out HealthController healthController))
        {
            healthController.TakeDamage(damage);
            
            if(statusModifier)
            {
                var statusEffectController = healthController.GetComponentInChildren<StatusEffectController>();
                if(statusEffectController)
                    statusModifier.ApplyStatus(statusEffectController);
            }
        }

        isDestroying = true;
        Destroy(gameObject);
    }
}