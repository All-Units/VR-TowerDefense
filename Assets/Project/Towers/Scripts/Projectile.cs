using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage;
    private Rigidbody rb;
    public float speed = 20f;

    protected bool isDestroying = false;

    public bool willBurn;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * speed, ForceMode.VelocityChange);
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
            if(willBurn)
            {
                var statusEffectController = healthController.GetComponentInChildren<StatusEffectController>();
                if(statusEffectController)
                    statusEffectController.StartBurn();
            }
        }

        isDestroying = true;
        Destroy(gameObject);
    }
}