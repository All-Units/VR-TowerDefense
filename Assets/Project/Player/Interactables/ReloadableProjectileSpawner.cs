using System.Collections.Generic;
using UnityEngine;

public class ReloadableProjectileSpawner : ProjectileSpawner
{
    [SerializeField] private int maxAmmo;
    private int _currentAmmo;
    [SerializeField] List<GameObject> _LoadedAmmoModels = new List<GameObject>();
    protected new void Start()
    {
        base.Start();
        foreach (GameObject model in _LoadedAmmoModels)
        {
            model.SetActive(false);
        }

    }
    public override void Fire()
    {
        if(CheckCantFireModules()) return;
        if (XRPauseMenu.IsPaused) return;
        if (_currentAmmo <= 0) return;
        if (targeter != null)
            targeter.CapNumberOfTargets(_currentAmmo);
        
        base.Fire();
        int toFire = _currentAmmo - fired;
        for (int i = 0; i < toFire; i++)
        {
            var projectile = Instantiate(projectilePrefab, startPoint.position, startPoint.rotation, null);
            Vector3 randSphere = Random.insideUnitSphere * 0.2f;
            Vector3 forward = projectile.transform.forward;
            forward += randSphere;
            projectile.transform.forward = forward;
            projectile.Fire();
            if (targeter && projectile.TryGetComponent(out GuidedMissileController guidedMissile))
            {
                guidedMissile.targeter = targeter;
                guidedMissile.index = -1;
            }
            projectile.gameObject.DestroyAfter(15f);
            fired++;
        }
        _currentAmmo = 0;
        foreach (GameObject model in _LoadedAmmoModels)
        {
            model.SetActive(false);
        }

    }

    public void Reload()
    {
        if (_currentAmmo >= maxAmmo)
        {
            _currentAmmo = maxAmmo;
            return;
        }
        if (_LoadedAmmoModels.Count > 0 && _LoadedAmmoModels.Count > _currentAmmo)
        {
            _LoadedAmmoModels[_currentAmmo].SetActive(true);
        }
        _currentAmmo++;
    }
}