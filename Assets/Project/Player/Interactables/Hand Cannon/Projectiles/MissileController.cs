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

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        upwardsCeiling += transform.position.y;
    }

    public void FireReachCruisingAltitude()
    {
        _cruisingCoroutine = StartCoroutine(ReachCruisingAltitude());
    }

    public void HitTarget(Transform target)
    {
        if(_cruisingCoroutine != null)
            StopCoroutine(_cruisingCoroutine);

        StartCoroutine(PursueTarget(target));
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

        if (cluster)
        {
            var enemies = FindObjectsOfType<Enemy>().ToList();
            Debug.Log($"Enemies found {enemies.Count}");

            for (int i = 0; i < 5; i++)
            {
                var newMissile = Instantiate(prefab,
                    transform.position +
                    new Vector3(Random.Range(-15, 15), Random.Range(-15, 15), Random.Range(-15, 15)),
                    Quaternion.identity);
                newMissile.HitTarget(enemies.GetRandom().transform);
            }
        }

        var enemy = FindObjectOfType<Enemy>();
        if (enemy)
            StartCoroutine(PursueTarget(enemy.transform));
    }

    IEnumerator PursueTarget(Transform target)
    {
        yield return new WaitForSeconds(pursueWaitTime);
        
        if(rb == null)
            rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;
        
        flamesVFX.Play(true);

        while (true)
        {
            if (target == null)
            {
                var e = FindObjectsOfType<Enemy>().ToList();
                if (e.Count > 0)
                    target = e.GetRandom().transform;
                else { 
                    target = null; 
                    yield return null;
                    continue; }
            }
                
            
            transform.LookAt(target);

            rb.AddForce(transform.forward * (moveSpeed * Time.deltaTime));

            if (rb.velocity.magnitude > maxSpeed)
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
            yield return null;
        }
    }

    public void SetTarget(Enemy target)
    {
        
    }
}