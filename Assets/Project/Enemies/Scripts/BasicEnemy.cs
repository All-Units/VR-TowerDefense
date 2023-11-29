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
    public GameObject headPrefab;
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
        _hc.onDeath.AddListener(OnDeath);
        _anim = GetComponentInChildren<Animator>();
        if (nextWaypoint == null)
        {
            Debug.Log($"{gameObject.name} was null ", gameObject);
            return;
        }
        else
        {
            _target = nextWaypoint.GetPoint();
        }
        
        Vector3 dir = nextWaypoint.nextPoint.transform.position - nextWaypoint.transform.position;
        dir.y = 0f;
        transform.rotation = Quaternion.LookRotation(dir);

        //We have a rigidbody, a health controller, and animator
        //Perform OnSpawn logic
        OnSpawn();
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
            //footstepSFXController.Stop();
            _MoveToAttack();
            return;
        }
        //If we're at the end, do not move
        if (reachedEnd)
        {
            movedLastFrame = false;
            //footstepSFXController.Stop();
            _zeroVelocity();
            return;
        }
        
        movedLastFrame = true;
        _moveLoop();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        //Our enemy just encountered a tower
        if (other.CompareTag("Tower"))
        {
            //print("Encountered tower, killing");
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
    void OnSpawn()
    {
        if (EnemyManager.Enemies.Contains(this) == false)
            EnemyManager.Enemies.Add(this);
        EnemyManager.EnemyCount++;
        EnemyManager.GregSpawned();
    }
    /// <summary>
    /// Logic on Greg death
    /// </summary>
    void OnDeath()
    {
        if (EnemyManager.Enemies.Contains(this))
            EnemyManager.Enemies.Remove(this);
        EnemyManager.EnemyCount--;
        EnemyManager.GregKilled();
    }

    private bool addedToMoney = false;

    public void Die()
    {
        if (addedToMoney == false && CurrencyManager.instance)
        {
            CurrencyManager.instance.CurrentMoney += killValue;
            addedToMoney = true;
            Minimap.RemoveHead(this);
            //EnemySpawner.RemoveEnemy(this);
                
        }
        reachedEnd = true;
        Destroy(gameObject);


    }

    #endregion
    
    #region HelperFunctions
    public float distanceToTarget;
    private Vector3 _target;
    void _moveLoop()
    {
        if(nextWaypoint == null) return;
        
        /*//Cache our pos, target pos, and distance to target 
        _target = nextWaypoint.GetPoint();
        pos.y = 0f;
        _target.y = 0f;
        float distanceToGoal = Vector3.Distance(pos, nextWaypoint.goal.position);
        //If we are closer to goal than the current waypoint, skip it
        while (nextWaypoint.nextPoint && distanceToGoal < nextWaypoint.DistanceToGoal)
            nextWaypoint = nextWaypoint.GetNext();
        distanceToTarget = distance;*/
        
        Vector3 pos = transform.position;
        float distance = Vector3.Distance(pos, _target);
        
        //If we've reached our current target
        if (distance <= targetTolerance)
        {
            //Get the next target
            nextWaypoint = nextWaypoint.GetNext();
            //If there is a next target
            if (nextWaypoint != null)
            {
                _target = nextWaypoint.GetPoint();
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
            Vector3 velocity = Vector3.Normalize(_target - pos) * moveSpeed;
            velocity.y = 0f;
            _rb.velocity += velocity;
        }
        _rotateTowards(pos, _target);
        
    }

    void _MoveToAttack()
    {
        if (attacking == false || currentTarget == null)
            return;
        Vector3 pos = transform.position;
        Vector3 target = currentTarget.GetPosition();
        _rotateTowards(pos, target);
        _zeroVelocity();
        if (Vector3.Distance(pos, target) > 1)
        { 
            Vector3 velocity = Vector3.Normalize(target - pos) * moveSpeed;
            velocity.y = 0f;
            _rb.velocity += velocity;
        }
        
    }
    //We don't want to zero velocity in the y direction
    void _zeroVelocity()
    {
        //We don't want to zero velocity in the y direction
        Vector3 velocity = _rb.velocity;
        velocity.x = 0f;
        velocity.z = 0f;
        _rb.velocity = velocity;
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

    public void Footstep()
    {
        //AudioClip clip = footstepSFXController.GetClip();
        
        footstepSFXController.PlayClip();
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