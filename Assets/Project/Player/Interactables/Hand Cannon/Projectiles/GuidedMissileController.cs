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

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        target = targeter.GetEnemy(index);
        Enemy.OnDeath += OnDeath;
    }

    private void OnDeath(Enemy obj)
    {
        if (obj == target)
        {
            targeter.OnEnemyDeath(obj);
            targeter.GetEnemy(index);
        }
    }

    public void HitTarget()
    {
        StartCoroutine(PursueTarget());
    }

    IEnumerator PursueTarget()
    {
        yield return new WaitForSeconds(pursueWaitTime);
        
        if(rb == null)
            rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;
        
        flamesVFX.Play(true);

        while (true)
        {
            var targetRotation = Quaternion.Euler(90, 0, 0);
            if(targeter.IsTargeting())
                targetRotation = Quaternion.LookRotation((target.transform.position + Vector3.up) - transform.position);
            if (transform.rotation != targetRotation)
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            rb.AddForce(transform.forward * (moveSpeed * Time.deltaTime));

            if (rb.velocity.magnitude > maxSpeed)
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
            yield return null;
        }
    }
}