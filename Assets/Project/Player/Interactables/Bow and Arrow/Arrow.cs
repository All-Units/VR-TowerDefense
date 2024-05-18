using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class Arrow : Projectile, IPausable
{
    public UnityEvent OnDrawnBack;
    public UnityEvent OnRelease;

    private Rigidbody _rigidbody;
    private bool _inAir = false;
    
    private IPausableComponents _ipComponents = null;
    public IPausableComponents IPComponents { 
        get {
            if (_ipComponents == null) _ipComponents = this.GetPausableComponents();
            return _ipComponents; 
        } 
    }
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        PullInteraction.PullActionStarted += PullInteractionOnPullActionStarted;
        PullInteraction.PullActionReleased += PullInteractionOnPullActionReleased;
        Stop();
        OnInitPausable();
        GetComponent<XRGrabInteractable>().selectExited.AddListener(_OnDrop);
    }
    public bool IsNotched = false;
    void _OnDrop(SelectExitEventArgs a)
    {
        if (IsNotched == false)
            Destroy(gameObject);
    }
    public void OnInitPausable()
    {
        this.InitPausable();
    }
    public void OnDestroyPausable() { this.DestroyPausable(); }

    private void OnDestroy()
    {
        OnDestroyPausable();
        PullInteraction.PullActionStarted -= PullInteractionOnPullActionStarted;
        PullInteraction.PullActionReleased -= PullInteractionOnPullActionReleased;

    }

    private void PullInteractionOnPullActionStarted()
    {
        OnDrawnBack?.Invoke();
    }
    private void PullInteractionOnPullActionReleased(float obj, TowerPlayerWeapon towerPlayerWeapon)
    {
        PullInteraction.PullActionStarted -= PullInteractionOnPullActionStarted;
        PullInteraction.PullActionReleased -= PullInteractionOnPullActionReleased;
        OnRelease?.Invoke();
        Fire(obj);
        playerWeapon = towerPlayerWeapon;
    }

    public void Fire()
    {
        Fire(1);
    }

    private void Fire(float obj)
    {
        startPos = transform.position;
        gameObject.transform.parent = null;
        _inAir = true;
        particles.gameObject.SetActive(true);
        SetPhysics(true);

        var force = transform.forward * obj * speed;
        _rigidbody.AddForce(force, ForceMode.Impulse);
        StartCoroutine(RotateWithVelocity());
    }

    private IEnumerator RotateWithVelocity()
    {
        yield return new WaitForFixedUpdate();
        while (_inAir)
        {
            if (isPaused == false) 
            { 
                var newRot = Quaternion.LookRotation(_rigidbody.velocity, transform.up);
                transform.rotation = newRot;
            }
            yield return null;
        }
    }

    private void Stop()
    {
        _inAir = false;
        particles.gameObject.SetActive(false);
        SetPhysics(false);
    }

    private void SetPhysics(bool b)
    {
        _rigidbody.useGravity = b;
        _rigidbody.isKinematic = !b;
    }

    bool isPaused = false;
    void IPausable.OnPause()
    {
        this.BaseOnPause();
        isPaused = true;
    }

    void IPausable.OnResume()
    {
        this.BaseOnResume();
        isPaused = false;
    }
}