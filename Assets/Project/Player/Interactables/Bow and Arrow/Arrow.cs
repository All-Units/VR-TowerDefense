using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Arrow : MonoBehaviour, IPausable
{
    public int damage;
    public float speed = 10f;

    public UnityEvent OnDrawnBack;
    public UnityEvent OnRelease;
    public UnityEvent OnHit;

    private Rigidbody _rigidbody;
    private bool _inAir = false;
    private bool _isDestroying = false;
    [SerializeField] private ParticleSystem particles;
    public StatusModifier statusModifier;
    [SerializeField] private AudioClipController woodHit;
    [SerializeField] private AudioClipController enemyHit;
    public int RagdollForce = 20;
    Vector3 startPos;

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
    private void PullInteractionOnPullActionReleased(float obj)
    {
        PullInteraction.PullActionStarted -= PullInteractionOnPullActionStarted;
        PullInteraction.PullActionReleased -= PullInteractionOnPullActionReleased;
        OnRelease?.Invoke();
        Fire(obj);
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

    private void OnCollisionEnter(Collision other)
    {
        //Debug.Log($"Hit {other.gameObject}", other.gameObject);
        if (_isDestroying) return;
        
        var colliderGameObject = other.collider.gameObject;
        Vector3 pos = transform.position;
        HealthController healthController = colliderGameObject.GetComponentInParent<HealthController>();
        Enemy e = colliderGameObject.GetComponentInParent<Enemy>();
        if (healthController && e)
        {
            healthController.TakeDamageFrom(damage, startPos);
            
            if(statusModifier)
            {
                var statusEffectController = healthController.GetComponentInChildren<StatusEffectController>();
                if(statusEffectController)
                    statusModifier.ApplyStatus(statusEffectController);
            }

            enemyHit.PlayClipAt(pos);
            
        }
        else
        {
            woodHit.PlayClipAt(pos);
        }

        _isDestroying = true;
        particles.transform.SetParent(null);
        particles.Stop();

        OnHit?.Invoke();
        Destroy(gameObject);
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