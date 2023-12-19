using UnityEngine;

public class ReloadableProjectileSpawner : ProjectileSpawner
{
    [SerializeField] private int maxAmmo;
    private int _currentAmmo;

    public override void Fire()
    {
        if(CheckCantFireModules()) return;

        if (_currentAmmo <= 0) return;
        
        _currentAmmo--;
        base.Fire();
    }

    public void Reload()
    {
        _currentAmmo = maxAmmo;
    }
}