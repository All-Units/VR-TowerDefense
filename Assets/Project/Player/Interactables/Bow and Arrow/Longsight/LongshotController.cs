using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class LongshotController : MonoBehaviour
{
    [SerializeField] private SlideInteraction slideInteraction;
    [SerializeField] private Projectile projectile;
    [SerializeField] private Transform projectileSpawnpoint;
    private Projectile _loadedProjectile;
    public GameObject handle;
    public float colDelay = 0.3f;

    public UnityEvent OnFire;

    public void Fire()
    {
        if(_loadedProjectile == false) return;
        
        _loadedProjectile.Fire();
        _loadedProjectile = null;
        OnFire?.Invoke();
    }

    public void Load()
    {
        _loadedProjectile = Instantiate(projectile, projectileSpawnpoint);
        StartCoroutine(_delayEnable());
    }
    IEnumerator _delayEnable()
    {
        yield return new WaitForSeconds(colDelay);
        handle.SetActive(true);
    }
}
