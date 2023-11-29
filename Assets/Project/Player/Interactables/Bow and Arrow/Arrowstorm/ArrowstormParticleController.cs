using UnityEngine;

public class ArrowstormParticleController : MonoBehaviour
{
    private void OnParticleCollision(GameObject other)
        {
            if (other.TryGetComponent(out HealthController healthController))
            {
                healthController.TakeDamage(1);
            }
        }
}
