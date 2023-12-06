using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Projectile : MonoBehaviour
{
    public int damage;
    private Rigidbody rb;
    public float speed = 20f;
    public float RagdollForce = 20f;

    protected bool isDestroying = false;

    public StatusModifier statusModifier;
    
    [SerializeField] protected AudioClipController _hitEnemy;
    [SerializeField] protected AudioClipController _hitGround;

    [SerializeField] private GameObject flyingVFX;
    Vector3 startPos;
    private void Awake()
    {
        startPos = transform.position;
    }
    public void Fire()
    {
        transform.SetParent(null);
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.AddForce(transform.forward * speed, ForceMode.VelocityChange);

        if(flyingVFX)
            flyingVFX.SetActive(true);
        Destroy(gameObject, 10f);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (isDestroying) return;

        if (other.collider.isTrigger) return;
        
        OnCollision(other.collider);
    }

    protected virtual void OnCollision(Collider other)
    {

        Vector3 pos = transform.position;
        BasicEnemy e = other.GetComponentInParent<BasicEnemy>();
        var healthController = other.GetComponentInParent<HealthController>();
        //If we just hit an enemy
        if (healthController != null && e != null)
        {
            healthController.TakeDamage(damage);
            
            if(statusModifier)
            {
                var statusEffectController = healthController.GetComponentInChildren<StatusEffectController>();
                if(statusEffectController)
                    statusModifier.ApplyStatus(statusEffectController);
            }
            if (_hitEnemy)
                _hitEnemy.PlayClipAt(pos);
            if (healthController.isDead)
            {
                Vector3 dir = pos - startPos;
                dir.y = 0f; dir = dir.normalized;
                dir.y = 1f;
                a = pos;

                dir *= RagdollForce;
                dir = Vector3.ClampMagnitude(dir, 400f);
                b = pos + dir.normalized * 4;
                e.RB.AddForce(dir, ForceMode.Impulse);
                _rb = e.RB;
            }
        }
        else
        {
            if (_hitGround)
                _hitGround.PlayClipAt(pos);
        }

        isDestroying = true;
        Destroy(gameObject);
    }
    
    protected Rigidbody _rb;
    protected Vector3 a = Vector3.zero;
    protected Vector3 b = Vector3.zero;
    private void OnDrawGizmos()
    {
        if (a == Vector3.zero || b == Vector3.zero) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawSphere(b, .5f);
        if (_rb != null) { 
            //print($"rb velocity: {_rb.velocity}");
            
        }
    }
}