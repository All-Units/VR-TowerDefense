using UnityEngine;

public class ArrowstormParticleController : MonoBehaviour
{
    [SerializeField] private int damage;
    private void OnParticleCollision(GameObject other)
        {
            if (other.TryGetComponent(out HealthController healthController))
            {
                healthController.TakeDamage(damage);
            }
        }
}
