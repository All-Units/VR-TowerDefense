using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider))]
public class AmmoReceiver : MonoBehaviour
{
    [SerializeField] private ReloadableProjectileSpawner reloadable;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Ammo ammo)) return;
        
        reloadable.Reload();
        Destroy(other.gameObject);
    }
}