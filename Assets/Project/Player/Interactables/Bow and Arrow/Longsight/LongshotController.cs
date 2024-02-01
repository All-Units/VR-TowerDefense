using UnityEngine;
using UnityEngine.Events;

public class LongshotController : MonoBehaviour
{
    [SerializeField] private SlideInteraction slideInteraction;
    [SerializeField] private Projectile projectile;
    [SerializeField] private Transform projectileSpawnpoint;
    private Projectile _loadedProjectile;

    public UnityEvent OnFire;

    public void Fire()
    {
        if(_loadedProjectile == false) return;
        if (XRPauseMenu.IsPaused) return;
        _loadedProjectile.Fire();
        _loadedProjectile = null;
        OnFire?.Invoke();
    }

    public void Load()
    {
        _loadedProjectile = Instantiate(projectile, projectileSpawnpoint);
    }
}
