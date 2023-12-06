using System.Collections;
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
                var dmg = Mathf.FloorToInt(damage * damageDropOff.Evaluate(Mathf.Clamp01(radius)));
                healthController.TakeDamage(dmg);
                //We killed Greg! Apply force between us and their center of gravity
                if (healthController.isDead && colliderGameObject.TryGetComponent(out BasicEnemy enemy))
                {
                    enemy.FlingRagdoll(pos);
                    
                }
            }
        }
        if(hitParticles)
        {
            var particles = Instantiate(hitParticles, pos, Quaternion.identity);
            Destroy(particles, 2f);
        }
        if (_audioClipController)
            _audioClipController.PlayClip();
        AudioPool.PlaySoundAt(_audioClipController.GetClip(), pos);
        isDestroying = true;
        Destroy(gameObject);
    }
   
    
}