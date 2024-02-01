using System;
using System.Collections;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

public class Projectile : MonoBehaviour, IPausable
{
    public int damage;
    private Rigidbody rb;
    public float speed = 20f;

    protected bool isDestroying = false;

    public StatusModifier statusModifier;
    
    [SerializeField] protected AudioClipController _hitEnemy;
    [SerializeField] protected AudioClipController _hitGround;

    [SerializeField] private GameObject flyingVFX;

    public UnityEvent OnFire;
    public UnityEvent OnHit;
    Vector3 startPos;
    void Awake()
    {
        OnInitPausable();
    }
    
    public void OnInitPausable()
    {
        this.InitPausable();
    }
    protected virtual void OnDestroy()
    {
        OnDestroyPausable();
        
    }
    public void OnDestroyPausable() { this.DestroyPausable(); }
    public void Fire()
    {
        transform.SetParent(null);
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.AddForce(transform.forward * speed, ForceMode.VelocityChange);

        if(flyingVFX)
            flyingVFX.SetActive(true);
        gameObject.DestroyAfter(20f);
        //StartCoroutine(gameObject._DestroyAfter(20f));
        //Destroy(gameObject, 20f);
        startPos = transform.position;
        OnFire?.Invoke();
    }
    

    private void OnCollisionEnter(Collision other)
    {
        if (isDestroying) return;

        if (other.collider.isTrigger) return;
        
        OnCollision(other.collider);
    }
    bool _isFireball => gameObject.name.ToLower().Contains("fireball");

    private IPausableComponents _ipComponents = null;
    public IPausableComponents IPComponents
    {
        get
        {
            if (_ipComponents == null) _ipComponents = this.GetPausableComponents();
            return _ipComponents;
        }
    }

    protected virtual void OnCollision(Collider other)
    {
        //var colliderGameObject = other.gameObject;
        var healthController = other.GetComponentInParent<HealthController>();  
        if (healthController != null)
        {
            Vector3 pos = transform.position;
            healthController.TakeDamageFrom(damage, startPos);
           

            ApplyEffects(healthController);
            
            // Todo Refactor out to event based
            if (_hitEnemy)
                _hitEnemy.PlayClipAt(transform.position);
        }
        else
        {
            if (gameObject.name.ToLower().Contains("fireball"))
                print($"Hit {other.gameObject}, NOT HEALTH CONTROLLER");
            // Todo Refactor out to event based
            if (_hitGround)
                _hitGround.PlayClipAt(transform.position);
        }
        //We hit a trigger that didn't have a HC
        if (other.isTrigger && healthController == null) return;
        OnHit?.Invoke();
        isDestroying = true;
        Destroy(gameObject);
    }

    protected void ApplyEffects(HealthController healthController)
    {
        if (statusModifier)
        {
            var statusEffectController = healthController.GetComponentInChildren<StatusEffectController>();
            if (statusEffectController)
                statusModifier.ApplyStatus(statusEffectController);
        }
    }

    void IPausable.OnPause()
    {
        this.BaseOnPause();
        
    }
    protected int lastFrameResumed = 0;
    void IPausable.OnResume()
    {
        this.BaseOnResume();
        lastFrameResumed = Time.frameCount;
    }
}