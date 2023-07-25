using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class BasicEnemy : Enemy
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float targetTolerance = 1f;
    [SerializeField] private float rotateDamping = 1f;
    [SerializeField] private AudioSource _audioSource;
    private Rigidbody _rb;

    public PathPoint nextWaypoint;
    public Tower currentTarget;
    private bool hasTarget = false;
    private bool reachedEnd = false;

    private Animator _anim;

    public int damage;
    public string equipment = "";

    #region UnityEvents

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
        //If we have a valid target, attack them and do not move
        if (attacking)
        {
            _zeroVelocity();
            return;
        }
        //If we're at the end, do not move
        if (reachedEnd)
        {
            _zeroVelocity();
            return;
        }

        _moveLoop();


    }
    
    private void OnTriggerEnter(Collider other)
    {
        //Our enemy just encountered a tower
        if (other.CompareTag("Tower"))
        {
            print("Encountered tower, killing");
            currentTarget = other.GetComponent<Tower>();
            currentTarget.healthController.OnDeath += OnTargetDeath;
            _zeroVelocity();
            _anim.SetTrigger(_getAttackAnimString());
            attacking = true;
        }
    }


    #endregion

    #region LifeCycle

    public void Die()
    {
        Destroy(gameObject);
    }

    #endregion
    
    #region HelperFunctions

    void _moveLoop()
    {
        //Cache our pos, target pos, and distance to target
        Vector3 pos = transform.position;
        Vector3 nextPos = nextWaypoint.transform.position;
        float distance = Vector3.Distance(pos, nextPos);
        
        //If we've reached our current target
        if (distance <= targetTolerance)
        {
            //Get the next target
            nextWaypoint = nextWaypoint.nextPoint;
            //If there is a next target
            if (nextWaypoint != null)
            {
                nextPos = nextWaypoint.transform.position;
            }
            else
            {
                reachedEnd = true;
                Victory();
            }
            
            
        }
        _zeroVelocity();
        //Move towards next target
        if (reachedEnd == false)
        {
            _rb.velocity = Vector3.Normalize(nextPos - pos) * moveSpeed;
        }
        
        //Rotate to face our next target
        Vector3 dir = nextPos - pos;
        dir.y = 0f;
        var rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotateDamping * Time.deltaTime);
    }
    void _zeroVelocity()
    {
        _rb.velocity = Vector3.zero;
    }
    
    bool attacking = false;

    /// <summary>
    /// When the enemy's attack impacts the tower
    /// </summary>
    public void Impact()
    {

        if (currentTarget == null)
        {
            currentTarget = null;
            attacking = false;
            _anim.SetTrigger("run");
            return;
        }

        _audioSource.Play();
        print("Hitting tower!");
        currentTarget.healthController.TakeDamage(damage);
        if (false)
        {
            print("Killed tower, resuming run");
            Destroy(currentTarget.gameObject);
            currentTarget = null;
            _anim.SetTrigger("run");
            attacking = false;
        }
    }

    IEnumerator _attackLoop()
    {
        if (attacking)
            yield break;
        attacking = true;
        _anim.SetTrigger("attack");
        
        float attackTime = 4.57f;
        print("Started attack");
        while (true)
        {
            if (currentTarget == null)
            {
                currentTarget = null;
                _anim.SetTrigger("run");
                break;
            }

            currentTarget.healthController.TakeDamage(damage);
            
            yield return new WaitForSeconds(attackTime);
            attackTime = _anim.GetCurrentAnimatorStateInfo(0).length;
        }
        attacking = false;
    }

    void Victory()
    {
        _zeroVelocity();
        _anim.SetTrigger("victory");
    }

    private string _getAttackAnimString()
    {
        string s = "attack";
        if (equipment.Contains("sword"))
            s += "sword";
        return s;
    }

    private void OnTargetDeath()
    {
        print("Killed tower, resuming run");
        currentTarget = null;
        _anim.SetTrigger("run");
        attacking = false;
    }

    #endregion
}