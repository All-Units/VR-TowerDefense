using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage;
    private Rigidbody rb;
    public float speed = 20f;

    protected bool isDestroying = false;

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
        }

        isDestroying = true;
        Destroy(gameObject);
    }
}