using UnityEngine;

public class AOEProjectile : Projectile
{
    public float splashRadius = 5;
    public GameObject hitParticles;

    [SerializeField] private AnimationCurve damageDropOff;

    protected override void OnCollision(Collision other)
    {
        var hits = Physics.OverlapSphere(other.collider.transform.position, splashRadius, LayerMask.GetMask("Enemy"));
        Vector3 pos = transform.position;
        foreach (var hit in hits)
        {
            var colliderGameObject = hit.gameObject;

            if (colliderGameObject.TryGetComponent(out HealthController healthController))
            {
                var distance = Vector3.Distance(hit.ClosestPoint(pos), pos);
                var radius = distance/splashRadius;
                healthController.TakeDamage(Mathf.FloorToInt(damage * damageDropOff.Evaluate(Mathf.Clamp01(radius))));
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