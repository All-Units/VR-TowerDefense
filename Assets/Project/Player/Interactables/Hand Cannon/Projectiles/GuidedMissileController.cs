using System;
using System.Collections;
using UnityEngine;

public class GuidedMissileController : MonoBehaviour
{
    public float pursueWaitTime;
    public float rotationSpeed;
    public float moveSpeed;
    public float maxSpeed = 200;
    public Rigidbody rb;
    [SerializeField] private ParticleSystem flamesVFX;

    public GuidedMissileTargeter targeter;
    public Enemy target;
    public int index = 0;
    public static Action<Enemy> OnMissileFiredAt;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if(targeter)
            target = targeter.GetEnemy(index);
        Enemy.OnDeath += OnDeath;
        OnMissileFiredAt?.Invoke(target);
        StartCoroutine(ExplodeAfterSeconds(15f));
    }

    private void OnDeath(Enemy obj)
    {
        if (obj == target)
        {
            if (targeter)
            {
                targeter.OnEnemyDeath(obj);
                target = targeter.GetEnemy(index);
            }
            else
            {
                target = null;
            }
        }
    }

    public void HitTarget()
    {
        StartCoroutine(PursueTarget());
    }

    private IEnumerator PursueTarget()
    {
        yield return new WaitForSeconds(pursueWaitTime);
        
        if(rb == null)
            rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;
        
        flamesVFX.Play(true);

        while (true)
        {
            var targetRotation = transform.rotation; //Quaternion.Euler(90, 0, 0);
            
            if(target)
                targetRotation = Quaternion.LookRotation((target.transform.position + Vector3.up) - transform.position);
            if (transform.rotation != targetRotation)
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            rb.AddForce(transform.forward * (moveSpeed * Time.deltaTime));

            if (rb.velocity.magnitude > maxSpeed)
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
            yield return null;
        }
    }

    public void SetTarget(Enemy t)
    {
        target = t;
    }

    private IEnumerator ExplodeAfterSeconds(float sec)
    {
        float t = 0f;
        while (t < sec)
        {
            if (XRPauseMenu.IsPaused == false)
                t += Time.deltaTime;
            yield return null;
        }
        //yield return new WaitForSeconds(sec);
        
        if(TryGetComponent(out AOEProjectile aoeProjectile))
            aoeProjectile.ManualExplode();
    }
}