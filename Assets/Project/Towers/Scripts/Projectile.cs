using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage;
    private Rigidbody rb;
    public float speed = 20f;

    private bool isDestroying = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * speed, ForceMode.VelocityChange);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (isDestroying) return;

        var colliderGameObject = other.collider.gameObject;
        //Debug.Log($"Colliding with {colliderGameObject}!", colliderGameObject);
        if (colliderGameObject.TryGetComponent(out HealthController healthController))
        {
            healthController.TakeDamage(damage);
        }

        isDestroying = true;
        Destroy(gameObject);
    }
}

