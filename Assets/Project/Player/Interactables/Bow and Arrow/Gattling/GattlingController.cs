using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GattlingController : MonoBehaviour
{
    public float spinUpTime = 3.5f;
    public float maxSpinSpeed = 15f;
    public Transform barrels;
    public Projectile projectile;
    public Transform launchPoint;
    
    private Coroutine _spinRoutine;
    private bool _isActive;
    private float fireRate;

    public UnityEvent OnFire;
    [SerializeField] private OverheatModule overheatModule;

    private void Start()
    {
        overheatModule.OnOverHeat.AddListener(OnDeactivate);

    }
    private void OnEnable()
    {
        XRPauseMenu.OnPause += OnPause;
        XRPauseMenu.OnResume += OnResume;
    }
    private void OnDisable()
    {
        XRPauseMenu.OnPause -= OnPause;
        XRPauseMenu.OnResume -= OnResume;
    }
    public void OnActivate()
    {
        if(overheatModule.IsOverheated()
            || XRPauseMenu.IsPaused)
            return;
        
        _isActive = true;
        if (_spinRoutine == null)
            _spinRoutine = StartCoroutine(Spin());
    }

    public void OnDeactivate()
    {
        _isActive = false;
    }

    bool _cachedIsActive = false;
    void OnPause()
    {
        _cachedIsActive = _isActive;
        _isActive = false;
    }
    void OnResume()
    {
        _isActive = _cachedIsActive;
    }

    private IEnumerator Spin()
    {
        var t = 0f;
        do
        {
            t = _isActive ? t + Time.deltaTime : t - Time.deltaTime;
            t = Mathf.Clamp(t, 0, spinUpTime);
            
            var spinSpeed = Mathf.Lerp(0, maxSpinSpeed, t / spinUpTime);
            barrels.Rotate(Vector3.forward, spinSpeed);
            if (spinSpeed > maxSpinSpeed / 4 && _isActive)
            {
                fireRate += spinSpeed;

                if (fireRate >= 90)
                {
                    fireRate -= 90;

                    if (XRPauseMenu.IsPaused == false)
                    {
                        var p = Instantiate(projectile, launchPoint.position, launchPoint.rotation);
                        p.Fire();

                        OnFire?.Invoke();
                    }
                }
            }
            
            yield return null;
        } while (t > 0);

        _spinRoutine = null;
    }
}
