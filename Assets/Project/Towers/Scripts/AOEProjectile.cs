using UnityEngine;

public class AOEProjectile : Projectile
{
    public float splashRadius = 5;
    public GameObject hitParticles;

    [SerializeField] private AnimationCurve damageDropOff;
    [SerializeField] private AudioClipController _audioClipController;
    
    protected override void OnCollision(Collider other)
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
                healthController.TakeDamageFrom(dmg, pos);

                
                ApplyEffects(healthController);
            }
            BasicEnemy.FlingRagdoll(colliderGameObject, pos);
        }
        if(hitParticles)
        {
            var particles = Instantiate(hitParticles, pos, Quaternion.identity);
            Destroy(particles, 2f);
        }
        
        // Todo Refactor out to event based
        /*if (_audioClipController)
            _audioClipController.PlayClip();
        AudioPool.PlaySoundAt(_audioClipController.GetClip(), pos);*/
        
        OnHit?.Invoke();
        isDestroying = true;
        Destroy(gameObject);
    }
}