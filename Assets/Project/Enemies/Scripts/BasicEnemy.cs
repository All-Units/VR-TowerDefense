using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class BasicEnemy : Enemy
{
    [SerializeField] private int killValue;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float targetTolerance = 1f;
    [SerializeField] private float rotateDamping = 1f;
    [SerializeField] private AudioClipController hitSFXController;
    [SerializeField] private AudioClipController footstepSFXController;
    private Rigidbody _rb;

    public PathPoint nextWaypoint;
    public IEnemyTargetable currentTarget;
    private bool hasTarget = false;
    [SerializeField]
    public bool reachedEnd = false;

    public Animator _anim;

    public int damage;
    public string equipment = "";
    
    private HealthController _hc;

    #region UnityEvents

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _hc = GetComponent<HealthController>();
        _anim = GetComponentInChildren<Animator>();
        if (nextWaypoint == null)
        {
            Debug.Log($"{gameObject.name} was null ", gameObject);
            return;
        }   
        
        Vector3 dir = nextWaypoint.nextPoint.transform.position - nextWaypoint.transform.position;
        dir.y = 0f;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    private bool movedLastFrame = false;
    // Update is called once per frame
    void Update()
    {
        if (_hc.isDead) return;
        //If we have a valid target, attack them and do not move
        if (attacking)
        {
            movedLastFrame = false;
            footstepSFXController.Stop();
            _MoveToAttack();
            return;
        }
        //If we're at the end, do not move
        if (reachedEnd)
        {
            movedLastFrame = false;
            footstepSFXController.Stop();
            _zeroVelocity();
            return;
        }
        if (movedLastFrame == false)
            footstepSFXController.PlayClip();
        movedLastFrame = true;
        _moveLoop();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        //Our enemy just encountered a tower
        if (other.CompareTag("Tower"))
        {
            print("Encountered tower, killing");
            currentTarget = other.GetComponent<IEnemyTargetable>();
            if (currentTarget != null)
                currentTarget.GetHealthController().OnDeath += OnTargetDeath;
            _zeroVelocity();
            _anim.SetTrigger(_getAttackAnimString());
            attacking = true;
        }
    }


    #endregion

    #region LifeCycle

    private bool addedToMoney = false;
    public void Die()
    {
        if (addedToMoney == false && CurrencyManager.instance)
        {
            CurrencyManager.instance.CurrentMoney += killValue;
            addedToMoney = true;
        }
        reachedEnd = true;
        Destroy(gameObject);
    }

    #endregion
    
    #region HelperFunctions
    public float distanceToTarget;
    void _moveLoop()
    {
        //Cache our pos, target pos, and distance to target
        Vector3 pos = transform.position;
        Vector3 nextPos = nextWaypoint.transform.position;
        pos.y = 0f;
        nextPos.y = 0f;
        float distance = Vector3.Distance(pos, nextPos);
        float distanceToGoal = Vector3.Distance(pos, nextWaypoint.goal.position);
        //If we are closer to goal than the current waypoint, skip it
        while (nextWaypoint.nextPoint && distanceToGoal < nextWaypoint.DistanceToGoal)
            nextWaypoint = nextWaypoint.nextPoint;
        distanceToTarget = distance;
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
        _rotateTowards(pos, nextPos);
        
    }

    void _MoveToAttack()
    {
        if (attacking == false || currentTarget == null)
            return;
        Vector3 pos = transform.position;
        Vector3 target = currentTarget.GetPosition();
        _rotateTowards(pos, target);
        if (Vector3.Distance(pos, target) > 1)
            _rb.velocity = Vector3.Normalize(target - pos) * moveSpeed;
        else
        {
            _zeroVelocity();
        }
        
    }
    void _zeroVelocity()
    {
        _rb.velocity = Vector3.zero;
    }
    /// <summary>
    /// Rotates us towards the given target
    /// </summary>
    /// <param name="pos">our current position</param>
    /// <param name="nextPos">current target's position</param>
    void _rotateTowards(Vector3 pos, Vector3 nextPos)
    {
        //Rotate to face our next target
        Vector3 dir = nextPos - pos;
        dir.y = 0f;
        var rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotateDamping * Time.deltaTime);
    }
    [SerializeField]
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

        hitSFXController.PlayClip();
        currentTarget.GetHealthController().TakeDamage(damage);
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
        currentTarget = null;
        if (_anim)
            _anim.SetTrigger("run");
        attacking = false;
    }

    #endregion
}