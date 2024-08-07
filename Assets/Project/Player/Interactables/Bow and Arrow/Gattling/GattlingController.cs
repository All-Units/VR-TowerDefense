using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class GattlingController : MonoBehaviour
{
    public float spinUpTime = 3.5f;
    public float coolDownScalar = 2f;
    public float maxSpinSpeed = 15f;
    public Transform barrels;
    public Projectile projectile;
    public Transform launchPoint;
    
    private Coroutine _spinRoutine;
    private bool _isActive;
    private float fireRate;

    public UnityEvent OnFire;
    [SerializeField] private OverheatModule overheatModule;
    [SerializeField] private AudioSource _revSpinAudioSource;
    [SerializeField] private float pitchMax, pitchMin;
    private float currentPitch;
    private TowerPlayerWeapon playerWeapon;
    XRGrabInteractable grab;
    
    private void Start()
    {
        overheatModule.OnOverHeat.AddListener(OnDeactivate);
        playerWeapon = GetComponent<TowerPlayerWeapon>();
        grab = GetComponent<XRGrabInteractable>();
        grab.lastSelectExited.AddListener(_OnDrop);
    }
    void _OnDrop(SelectExitEventArgs a)
    {
        OnDeactivate();
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

    void OnPause()
    {
        _isActive = false;
    }
    void OnResume()
    {
        _isActive = false;
    }

    private IEnumerator Spin()
    {
        var t = 0f;

        _revSpinAudioSource.Play();
        do
        {
            t = _isActive ? t + Time.deltaTime : t - (Time.deltaTime * coolDownScalar);
            t = Mathf.Clamp(t, 0, spinUpTime); 
            
            var spinSpeed = Mathf.Lerp(0, maxSpinSpeed, t / spinUpTime);
            barrels.Rotate(Vector3.forward, spinSpeed);
            _revSpinAudioSource.pitch = Mathf.Lerp(pitchMin, pitchMax, spinSpeed / maxSpinSpeed);
            
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
                        p.playerWeapon = playerWeapon;

                        OnFire?.Invoke();
                    }
                }
            }
            
            yield return null;
        } while (t > 0);

        _spinRoutine = null;
        _revSpinAudioSource.Stop();
    }
}
