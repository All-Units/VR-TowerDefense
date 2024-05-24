using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class LongshotController : MonoBehaviour
{
    [SerializeField] private SlideInteraction slideInteraction;
    [SerializeField] private Projectile projectile;
    [SerializeField] private Transform projectileSpawnpoint;
    [SerializeField] private Projectile _loadedProjectile;

    public UnityEvent OnFire;
    private void Start()
    {
        _loadedProjectile = null;
    }

    public void Fire()
    {
        if(_loadedProjectile == null || _isCocked == false) return;
        if (XRPauseMenu.IsPaused) return;

        _loadedProjectile.Fire();
        _loadedProjectile = null;
        OnFire?.Invoke();
        _isCocked = false;
    }
    bool _isCocked = false;
    public void Load()
    {
        _isCocked = true;
        //_loadedProjectile = Instantiate(projectile, projectileSpawnpoint);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Do nothing if we already have an arrow
        if (_loadedProjectile != null) return;
        //We only care about touching arrows
        if (other.gameObject.name.ToLower().Contains("arrow") == false
            || other.gameObject.layer == 0) return;
        
        Arrow arrow = other.GetComponent<Arrow>();
        if (arrow == null) return;
        arrow.IsNotched = true;
        
        _loadedProjectile = Instantiate(projectile, projectileSpawnpoint);
        _loadedProjectile.gameObject.layer = 0;

        var grab = _loadedProjectile.GetComponent<XRGrabInteractable>();
        if (grab)
        {
            grab.enabled = false;
        }

        Destroy(other.gameObject);
    }

}
