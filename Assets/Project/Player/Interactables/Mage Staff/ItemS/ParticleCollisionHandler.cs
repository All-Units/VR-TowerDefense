using UnityEngine;

public class ParticleCollisionHandler : MonoBehaviour
{
    public int damage = 1;
    public StatusModifier statusModifier;

    private void OnParticleCollision(GameObject other)
    {
        if (other.TryGetComponent(out HealthController healthController))
        {
            healthController.TakeDamage(damage);

            ApplyEffects(healthController);
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
