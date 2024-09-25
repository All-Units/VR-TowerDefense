using System;
using System.Collections.Generic;
using Unity.Loading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

public class Projectile : DamageDealer, IPausable
{
    public float speed = 20f;
    protected bool isDestroying = false;
    
    [SerializeField, FormerlySerializedAs("enemyHit")] protected AudioClipController _hitEnemy;
    [SerializeField, FormerlySerializedAs("woodHit")] protected AudioClipController _hitGround;

    [SerializeField] private GameObject flyingVFX;
    [SerializeField] protected ParticleSystem particles;

    private Rigidbody rb;

    public UnityEvent OnFire;
    public UnityEvent OnHit;

    protected Vector3 startPos;
    protected float timeCreated = 0f;
    [Tooltip("Checks if there is an enemy within this far of the impact point")]
    [SerializeField] float _graceRadius = 0.1f;

    void Awake()
    {
        OnInitPausable();
        timeCreated = Time.time;
        
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
        
        startPos = transform.position;
        OnFire?.Invoke();
    }

    int _destroyedFrame = -1;

    int lastHitFrame = 0;
    private void OnCollisionEnter(Collision other)
    {
        //Early out if we're destroying, but not if destroying this frame
        if (isDestroying && Time.frameCount != _destroyedFrame) return;

        if (other.collider.isTrigger) return;
        Enemy enemy = other.gameObject.GetComponentInParent<Enemy>();
        
        //If we were created too recently (exeption for enemies)
        if (Time.time - timeCreated < 0.05f && enemy == null)
        {
            //(tower != null || grab != null) && 
            return;
        }
        //Debug.Log($"Projectile hit: {other.gameObject.name}. FC: {Time.frameCount}", other.gameObject);
        
        Projectile proj = other.collider.GetComponentInChildren<Projectile>();
        if (proj == null) proj = other.collider.GetComponentInParent<Projectile>();
        //Don't do anything if we hit another projectile
        if (proj != null) return;

        //sphere cast out by the grace radius
        Vector3 impactPos = other.GetContact(0).point;
        LayerMask mask = LayerMask.GetMask("Enemy");
        
        Vector3 pos = transform.position;
        Vector3 dir = transform.forward;
        RaycastHit hit;

        //There was an issue with hitting phantom ground, this checks if there is an enemy close in front
        if (Physics.Raycast(pos, dir, out hit, 5f, mask))
        {
            enemy = hit.collider.gameObject.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                OnCollision(hit.collider);
                return;
            }

        }

        OnCollision(other.collider);
        
    }

    private bool _isFireball => gameObject.name.ToLower().Contains("fireball");

    private IPausableComponents _ipComponents = null;
    public IPausableComponents IPComponents
    {
        get
        {
            if (_ipComponents == null) _ipComponents = this.GetPausableComponents();
            return _ipComponents;
        }
    }

    public ProjectileTower tower { get; set; }
    
    bool _hasHitEnemy = false;
    protected virtual void OnCollision(Collider other)
    {
        var healthController = other.GetComponentInParent<HealthController>();
        if (healthController != null)
        {
            if (_hasHitEnemy == false)
            {
                HitEnemy(healthController);
                _hasHitEnemy = true;
            }
            
        }
        else
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                Debug.LogError($"Unable to find health controller on a collider with \"Enemy\" layer");
            
            if (_hitGround)
                _hitGround.PlayClipAt(transform.position);
        }

        if (particles != null)
        {
            particles.transform.SetParent(null);
            particles.Stop();
        }
        
        //We hit a trigger that didn't have a HC
        if (other.isTrigger && healthController == null) return;
        
        OnHit?.Invoke();
        isDestroying = true;
        _destroyedFrame = Time.frameCount;
        Destroy(gameObject);
    }

    private void HitEnemy(HealthController healthController)
    {
        ApplyDamage(healthController, damage, startPos);
        ApplyEffects(healthController);

        // Todo Refactor out to event based
        if (_hitEnemy)
            _hitEnemy.PlayClipAt(transform.position);
    }

    public override void OnKill(Enemy enemy = null)
    {
        base.OnKill(enemy);
        if(tower != null)
            tower.OnKill(enemy);
        
        if(playerWeapon != null)
            playerWeapon.OnKill(enemy);
        
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



public class DamageDealer : MonoBehaviour
{
    public int damage;
    public DamageType damageType;
    public StatusModifier statusModifier;
    public TowerPlayerWeapon playerWeapon { get; set; }

    protected void ApplyDamage(HealthController healthController, int damageToApply, Vector3 pos)
    {
        if (healthController.TryGetComponent(out Enemy enemy))
            damageToApply = Mathf.FloorToInt(damageToApply * enemy.ApplyResistanceWeakness(new List<DamageType>() { damageType }));
        healthController.TakeDamageFrom(damageToApply, pos, this);
    }
    protected void ApplyEffects(HealthController healthController)
    {
        if (statusModifier)
        {
            var statusEffectController = healthController.GetComponentInChildren<StatusEffectController>();
            if (statusEffectController)
            {
                statusEffectController.damageSource = this;
                if (playerWeapon != null)
                {
                    statusEffectController.damageSource = playerWeapon.GetDamageDealer;
                }
                
                statusModifier.ApplyStatus(statusEffectController); 
            }

        }
    }

    public virtual void OnKill(Enemy enemy = null)
    {
        //Debug.Log($"Killed {enemy}");
    }
}