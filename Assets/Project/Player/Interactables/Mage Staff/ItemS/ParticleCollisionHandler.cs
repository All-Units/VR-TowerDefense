using UnityEngine;

public class ParticleCollisionHandler : MonoBehaviour
{
    public int damage = 1;
    public float _minTimeBetweenDamage = 0.1f;
    public StatusModifier statusModifier;
    private void OnParticleTrigger()
    {
        print("Particle entered trigger!");
    }
    float _lastDamageTime = 0f;
    private void OnParticleCollision(GameObject other)
    {
        //How long since the last damage
        float timeDelta = Time.time - _lastDamageTime;
        //If it's been too recent since we last did damage, return out
        if (timeDelta <= _minTimeBetweenDamage) return;

        if (other.TryGetComponent(out HealthController healthController))
        {
            healthController.TakeDamage(damage);

            ApplyEffects(healthController);
            _lastDamageTime = Time.time;
        }
    }
    
    private void ApplyEffects(HealthController healthController)
    {
        if (statusModifier)
        {
            var statusEffectController = healthController.GetComponentInChildren<StatusEffectController>();
            if (statusEffectController)
                statusModifier.ApplyStatus(statusEffectController);
        }
    }
}
