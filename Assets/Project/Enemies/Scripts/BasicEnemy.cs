using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class BasicEnemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float targetTolerance = 1f;
    [SerializeField] private float rotateDamping = 1f;
    private Rigidbody _rb;

    public PathPoint nextWaypoint;
    public Tower currentTarget;

    private Animator _anim;
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();
        Vector3 dir = nextWaypoint.nextPoint.transform.position - nextWaypoint.transform.position;
        dir.y = 0f;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    // Update is called once per frame
    void Update()
    {
        if (nextWaypoint == null)
        {
            _rb.velocity = Vector3.zero;
            return;
        }
            
        Vector3 pos = transform.position;
        
        Vector3 nextPos = nextWaypoint.transform.position;
        float distance = (Vector3.Distance(pos, nextPos));
        if (distance <= targetTolerance)
        {
            nextWaypoint = nextWaypoint.nextPoint;
            if (nextWaypoint != null)
            {
                nextPos = nextWaypoint.transform.position;
            }
            else
            {
                _anim.SetTrigger("attack");
            }
            
        }
        _rb.velocity = Vector3.zero;
        if (nextWaypoint != null)
        {
            _rb.velocity = Vector3.Normalize(nextPos - pos) * moveSpeed;
        }

        Vector3 dir = nextPos - pos;
        dir.y = 0f;
        var rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotateDamping * Time.deltaTime);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tower"))
        {
            
        }
    }
}
