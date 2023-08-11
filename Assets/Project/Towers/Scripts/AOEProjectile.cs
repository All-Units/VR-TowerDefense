using System.Linq;
using UnityEngine;

public class AOEProjectile : Projectile
{
    public float splashRadius = 5;
    
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
        
        isDestroying = true;
        Destroy(gameObject);
    }
}