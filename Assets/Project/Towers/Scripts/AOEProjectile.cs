using UnityEngine;

public class AOEProjectile : Projectile
{
    public float splashRadius = 5;
    public GameObject hitParticles;

    protected override void OnCollision(Collision other)
    {
        var hits = Physics.OverlapSphere(other.collider.transform.position, splashRadius, LayerMask.GetMask("Enemy"));
        
        foreach (var hit in hits)
        {
            var colliderGameObject = hit.gameObject;

            if (colliderGameObject.TryGetComponent(out HealthController healthController))
            {
                healthController.TakeDamage(damage);
            }
        }

        if(hitParticles)
        {
            var particles = Instantiate(hitParticles, other.collider.transform.position, Quaternion.identity);
            Destroy(particles, 2f);
        }
        
        isDestroying = true;
        Destroy(gameObject);
    }
}