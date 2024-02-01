using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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

    [SerializeField] private float selfDestructTime = 20f;
    
    
    void Awake()
    {
        OnInitPausable();
    }
    void OnDestroy()
    {
        OnDestroyPausable();
    }
    public void OnInitPausable()
    {
        this.InitPausable();
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
        StartCoroutine(gameObject._DestroyAfter(selfDestructTime));
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
            if (_isFireball)
                print($"Hit {healthController.gameObject}");
            //healthController.TakeDamage(damage);

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

    void IPausable.OnResume()
    {
        this.BaseOnResume();
    }
}