using System.Collections;
using System.Linq;
using UnityEngine;

public class MissileController : MonoBehaviour
{
    public float orbitalWaitTime;
    public float pursueWaitTime;
    public float rotationSpeed;
    public float moveSpeed;
    public float maxSpeed = 200;
    public Rigidbody rb;
    [SerializeField] private ParticleSystem flamesVFX;
    public bool cluster;
    public float upwardsCeiling = 100f;

    private Coroutine _cruisingCoroutine;
    public MissileController prefab;
    private Enemy target;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        upwardsCeiling += transform.position.y;
    }

    public void FireReachCruisingAltitude()
    {
        _cruisingCoroutine = StartCoroutine(ReachCruisingAltitude());
    }

    private void MoveToTarget(Quaternion targetRotation)
    {
        if (transform.rotation != targetRotation)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        rb.AddForce(transform.forward * (moveSpeed * Time.deltaTime));

        if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    }

    IEnumerator ReachCruisingAltitude()
    {
        rb.useGravity = false;

        yield return new WaitForSeconds(orbitalWaitTime);

        flamesVFX.Play(true);

        while (transform.position.y < upwardsCeiling)
        {
            MoveToTarget(Quaternion.Euler(-90, 0, 1));
            yield return null;
        }

        rb.velocity = Vector3.zero;

        _cruisingCoroutine = null;

        if (target)
            StartCoroutine(PursueTarget());
    }

    private IEnumerator PursueTarget()
    {
        yield return new WaitForSeconds(pursueWaitTime);
        maxSpeed *= 2;
        moveSpeed *= 2;
        
        if(rb == null)
            rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;
        
        flamesVFX.Play(true);

        while (true)
        {
            var targetRotation = transform.rotation;
            
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

    public void SetTarget(Enemy target)
    {
        this.target = target;
    }
}