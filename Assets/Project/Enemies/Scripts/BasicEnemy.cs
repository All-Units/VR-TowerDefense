using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public class BasicEnemy : Enemy
{
    [SerializeField] EnemyDTO enemyDTO;


    public GameObject headPrefab;


    private string equipment = "";

    #region InternalVariables
    [HideInInspector] EnemyType enemyType;
    public Vector3 pos;
    [HideInInspector]
    public PathPoint nextWaypoint;
    public IEnemyTargetable currentTarget;
    private bool hasTarget = false;
    [HideInInspector]
    public bool reachedEnd = false;
    private int killValue;
    private float moveSpeed = 5f;
    private float targetTolerance = 1f;
    private float rotateDamping = 1f;
    Vector3 _initialHipsPos;
    float hitboxRadius => _capsuleCollider.radius;
    int damage;


    int targetingRange;

    #endregion

    #region ComponentReferences
    [Header("Component References")]
    [SerializeField]
    private Animator _anim;
    [SerializeField]
    private HealthController _hc;
    [SerializeField]
    private Rigidbody _rb;
    public Rigidbody RB => _rb;
    [SerializeField] private SphereCollider _sphereCollider;
    [Tooltip("The physics hitbox")]
    [SerializeField] private CapsuleCollider _capsuleCollider;
    [SerializeField] private AudioClipController hitSFXController;
    [SerializeField] private AudioClipController footstepSFXController;
    [SerializeField] private Rigidbody _hipsRB;
    [SerializeField] private LayerMask _ignoreGregLayer;
    public Transform CenterOfGravity;

    #endregion

    #region UnityEvents

    void Start()
    {
        //Fill values from DTO
        killValue = enemyDTO.KillValue;
        moveSpeed = enemyDTO.MoveSpeed;
        moveSpeed += Random.Range(-enemyDTO.MoveSpeedVariance, enemyDTO.MoveSpeedVariance);
        damage = enemyDTO.Damage;
        targetTolerance = enemyDTO.targetTolerance;
        rotateDamping = enemyDTO.rotateDamping;

        targetingRange = Random.Range(enemyDTO.MinRange, enemyDTO.MaxRange);
        _sphereCollider.radius = targetingRange;
        //Add OnDeath logic to healthcontroller
        _hc.onDeath.AddListener(OnDeath);
        _hc.SetMaxHealth(enemyDTO.Health);
        //Face which direction our first
        _FaceWaypoint();

        //We have a rigidbody, a health controller, and animator
        //Perform OnSpawn logic
        OnSpawn();

        _initialHipsPos = _hipsRB.transform.localPosition;
    }
    public float ManualSpeed = 1f;

    // Update is called once per frame
    void Update()
    {
        //State machine logic
        _EnemyStateMachine();
    }
    private bool movedLastFrame = false;

    private void OnTriggerEnter(Collider other)
    {
        var enemy = other.GetComponent<BasicEnemy>();
        if (enemy != null)
        {
            HandleCollision(enemy);
            return;
        }
        var tower = other.GetComponent<IEnemyTargetable>();
        if (tower != null)
        {
            _targets.Add(tower);
            TargetList.Add(other.gameObject);
        }
        return;
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
    private void OnTriggerExit(Collider other)
    {
        var tower = other.GetComponent<IEnemyTargetable>();
        if (tower != null)
        {
            _targets.Remove(tower);
            TargetList.Remove(other.gameObject);
        }
    }


    #endregion

    #region StateMachine

    private List<GameObject> TargetList = new List<GameObject>();
    Vector3 lastPos;
    /// <summary>
    /// Called every frame, this handles the logic tree for enemies
    /// </summary>
    private void _EnemyStateMachine()
    {
        pos = transform.position;
        Attacking = false;
        float distance = Utilities.FlatDistance(pos, _target);
        distanceToTarget = distance;
        //If we are dead, perform no further logic
        if (_hc.isDead) return;
        
        //There is at least one target in range
        if (_targets.Count > 0)
        {
            _AttackState();
            return;
        }
        //If we're at the end, do not move
        if (reachedEnd)
        {
            movedLastFrame = false;
            _zeroVelocity();
            return;
        }

        _MoveState();
        //_moveLoop();
    }

    /// <summary>
    /// The current list of towers in range
    /// </summary>
    private HashSet<IEnemyTargetable> _targets = new HashSet<IEnemyTargetable>();
    IEnumerator _targetSelector;
    /// <summary>
    /// Attacks the closest tower in range
    /// </summary>
    private void _AttackState()
    {
        //If we don't have a target, choose a new one
        if (currentTarget == null && _targets.Count > 0)
        {
            if (_targetSelector != null) return;
            _targetSelector = SelectNewTarget();
            StartCoroutine(_targetSelector);
            if (currentTarget == null) return;
        }
        float d = Utilities.FlatDistance(pos, _target);
        d -= hitboxRadius;
        if (d > enemyDTO.attackThreshold)
        {
            _Move();
        }
        else
        {
            listeningForFootstep = true;
            timeSinceLastAttack = Time.time - lastAttackTime;

            lastAttackTime = Time.time;

            Attacking = true;
            _rb.constraints = RigidbodyConstraints.FreezeAll;
            UpdateSpeed(0f);

        }

        //TO DO  do this!
    }

    [Header("Debug variables")]
    public bool Attacking = false;
    public bool listeningForFootstep = false;
    public float lastAttackTime = 0f;
    public float timeSinceLastAttack;
    public Vector3 MoveDir;
    private void _MoveState()
    {
        if (nextWaypoint == null) return;




        float distance = Utilities.FlatDistance(pos, _target);
        distanceToTarget = distance;
        //We've reached our current target
        if (distance <= enemyDTO.targetTolerance)
        {
            //Get the next target
            nextWaypoint = nextWaypoint.Next;
            //We've reached the Castle
            if (nextWaypoint == null)
            {
                EnemyVictory();
                return;
            }
            else
            {
                _target = nextWaypoint.GetPoint(hitboxRadius);
            }
        }
        _Move();

    }

    void _Move()
    {
        UpdateSpeed(1f);
        //Do not move if we are listening for a footstep
        if (listeningForFootstep)
            return;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        Vector3 dir = _target - pos;
        dir.y = 0;
        dir = dir.normalized;

        //Scale velocity
        dir *= moveSpeed;

        //Set our velocity in the horizontal plane
        Vector3 velocity = _rb.velocity;

        velocity.x = dir.x; velocity.z = dir.z;
        _rb.velocity = velocity;
        _rotateTowards(_target);

    }

    #endregion

    #region LifeCycle
    List<Collider> _ragdollColliders = new List<Collider>();
    void OnSpawn()
    {
        if (EnemyManager.Enemies.Contains(this) == false)
            EnemyManager.Enemies.Add(this);
        EnemyManager.EnemyCount++;
        EnemyManager.GregSpawned();
        foreach (Transform t in transform)
        {
            if (t == transform) continue;
            _ragdollColliders.AddRange(t.GetComponentsInChildren<Collider>().ToList());
        }
        foreach (var col in _ragdollColliders)
            col.enabled = false;
    }


    public void HandleCollision(BasicEnemy other)
    {
        float t = Time.time;
        //It has been less than a second since we collided with something
        //Or they hit something
        if (t - lastCollisionTime <= 1f
            || t - other.lastCollisionTime <= 1f)
            return;
        lastCollisionTime = t;
        other.lastCollisionTime = t;
        other.NewWaypoint(nextWaypoint, _target);
    }
    [HideInInspector]
    public float lastCollisionTime = 0f;

    /// <summary>
    /// Logic on Greg death
    /// </summary>
    void OnDeath()
    {
        //Turn off anim
        _anim.enabled = false;
        if (EnemyManager.Enemies.Contains(this))
            EnemyManager.Enemies.Remove(this);
        EnemyManager.EnemyCount--;
        EnemyManager.GregKilled();
        StartCoroutine(_FreezeLocalHipsPos());
        foreach (var col in _ragdollColliders)
            col.enabled = true;
    }
    /// <summary>
    /// After death, keeps the ragdoll centered on this GO
    /// </summary>
    /// <returns></returns>
    IEnumerator _FreezeLocalHipsPos()
    {
        var parent = _hipsRB.transform.parent;
        
        foreach (var collider in GetComponents<Collider>())
        {
            collider.excludeLayers = _ignoreGregLayer;
        }
        RB.constraints = RigidbodyConstraints.None;
        while (true)
        {
            
            RB.velocity = Vector3.ClampMagnitude(RB.velocity, enemyDTO.MaxRagdollForce);
            _hipsRB.velocity = RB.velocity;
            _hipsRB.transform.position = pos;
            
            yield return null;

        }
    }

    private bool addedToMoney = false;

    public void Die()
    {
        if (addedToMoney == false && CurrencyManager.instance)
        {
            CurrencyManager.GiveToPlayer(killValue);
            addedToMoney = true;
            Minimap.RemoveHead(this);
        }
        reachedEnd = true;
        Destroy(gameObject, enemyDTO.RagdollTime);
    }

    #endregion

    #region HelperFunctions
    /// <summary>
    /// Checks if a GO has an Enemy, and flings it if so
    /// </summary>
    /// <param name="go"></param>
    /// <param name="target"></param>
    public static void FlingRagdoll(GameObject go, Vector3 target)
    {
        var e = go.GetComponentInParent<BasicEnemy>();
        if (e)
            e.FlingRagdoll(target);
    }
    /// <summary>
    /// Applies force to the ragdoll towards the given world pos
    /// </summary>
    /// <param name="target"></param>
    public void FlingRagdoll(Vector3 target)
    {
        if (healthController.isDead == false) return;
        foreach (var col in GetComponents<Collider>())
            col.excludeLayers = _ignoreGregLayer;
        Vector3 dir = pos - target;
        dir.y = 0f; dir = dir.normalized;
        dir.y = 1f;
        dir = dir.normalized;

        dir *= enemyDTO.RagdollForce;
        dir = Vector3.ClampMagnitude(dir, enemyDTO.MaxRagdollForce);
        StartCoroutine(_ApplyForceContinuously(dir));
    }
    IEnumerator _ApplyForceContinuously(Vector3 dir, float time = 0.5f)
    {
        var t = Time.time;
        while (Time.time - t <= time) {
            RB.AddForce(dir);
            yield return null;
            Vector3 velocity = RB.velocity;
            float y = enemyDTO.MinRagdollYForce;
            if (velocity.y < y)
                velocity.y = y;
            RB.velocity = velocity;
            
        }
    }
    void UpdateSpeed(float speed)
    {
        speed = Mathf.Clamp01(speed);
        _anim.SetFloat("Speed", speed);
    }

    IEnumerator SelectNewTarget()
    {
        if (_targets.Count == 0)
            yield break;

        while (true)
        {
            _CullTargets();
            try
            {
                //Selects the closest tower in range
                var closest = _targets.OrderBy(t => Vector3.Distance(t.GetHealthController().transform.position, pos)).FirstOrDefault();
                _target = closest.GetHealthController().transform.position;
                currentTarget = closest;
                break;
            }
            catch (MissingReferenceException e) { }
            catch (NullReferenceException e) { }
            yield return null;

        }
        _targetSelector = null;
        currentTarget.GetHealthController().onDeath.AddListener(OnTargetDeath);
    }
    void _CullTargets()
    {
        var copy = _targets.ToArray();
        var valid = new List<IEnemyTargetable>();
        for (int i = 0; i < copy.Count(); i++)
        {
            var t = copy[i];
            try { var h = t.GetHealthController(); var x = h.transform.position; }
            catch (MissingReferenceException e) { continue; }
            catch (NullReferenceException e) { continue; }
            valid.Add(t);
        }


        _targets = valid.ToHashSet();
    }

    public float distanceToTarget;
    public Vector3 _target;
    void _FaceWaypoint()
    {
        if (nextWaypoint == null) return;
        _target = nextWaypoint.GetPoint(hitboxRadius);
        Vector3 dir = nextWaypoint.nextPoint.transform.position - nextWaypoint.transform.position;
        dir.y = 0f;
        transform.rotation = Quaternion.LookRotation(dir);

    }

    /// <summary>
    /// We don't want to zero velocity in the y direction
    /// </summary>
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
    void _rotateTowards(Vector3 nextPos)
    {
        //Rotate to face our next target
        Vector3 dir = nextPos - pos;
        dir.y = 0f;
        var rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotateDamping * Time.deltaTime);
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
            //_anim.SetTrigger("run");
            return;
        }
        hitSFXController.PlayClip();
        currentTarget.GetHealthController().TakeDamage(damage);
    }

    public void Footstep()
    {
        //AudioClip clip = footstepSFXController.GetClip();
        if (listeningForFootstep && Time.time - lastAttackTime < 0.5f)
        {
            return;
        }
        //if (listeningForFootstep) print("Was waiting for footstep, and we got one");
        if (listeningForFootstep)
            listeningForFootstep = false;
        footstepSFXController.PlayClip();
    }
    /// <summary>
    /// Logic for when the enemy reaches their goal
    /// </summary>
    void EnemyVictory()
    {
        reachedEnd = true;
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
        if (currentTarget != null)
            _targets.Remove(currentTarget);
        currentTarget = null;
        if (_anim)
            _anim.SetTrigger("run");
        attacking = false;

        //If that was the only tower, need to pick a new point
        if (_targets.Count == 0)
        {
            //_target = nextWaypoint.GetPoint(hitboxRadius);
            float _targetDistance = nextWaypoint.DistanceToGoalFrom(_target);
            float d = nextWaypoint.DistanceToGoalFrom(pos);
            do
            {
                if (nextWaypoint.nextPoint != null)
                    NewWaypoint(nextWaypoint.nextPoint, nextWaypoint.nextPoint.GetPoint(hitboxRadius));
                else
                    NewWaypoint(nextWaypoint, nextWaypoint.GetPoint(hitboxRadius));
                _targetDistance = nextWaypoint.DistanceToGoalFrom(_target);
                d = nextWaypoint.DistanceToGoalFrom(pos);
                
            } while (_targetDistance > d);

        }
        //print($"Target died, setting current target to null");
    }
    void NewWaypoint(PathPoint point, Vector3 target)
    {
        nextWaypoint = point;
        _target = target;
    }
    #endregion
    #region Legacy
    /// <summary>
    /// Core movement logic
    /// </summary>
    void _moveLoop()
    {
        if (nextWaypoint == null) return;
        movedLastFrame = true;
        /*//Cache our pos, target pos, and distance to target 
        _target = nextWaypoint.GetPoint();
        pos.y = 0f;
        _target.y = 0f;
        float distanceToGoal = Vector3.Distance(pos, nextWaypoint.goal.position);
        //If we are closer to goal than the current waypoint, skip it
        while (nextWaypoint.nextPoint && distanceToGoal < nextWaypoint.DistanceToGoal)
            nextWaypoint = nextWaypoint.GetNext();
        distanceToTarget = distance;*/

        pos = transform.position;
        float distance = Vector3.Distance(pos, _target);

        //If we've reached our current target
        if (distance <= targetTolerance)
        {
            //Get the next target
            nextWaypoint = nextWaypoint.Next;
            //If there is a next target
            if (nextWaypoint != null)
            {
                _target = nextWaypoint.GetPoint(hitboxRadius);
            }
            else
            {
                EnemyVictory();
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
        _rotateTowards(_target);

    }

    void _MoveToAttack()
    {
        if (attacking == false || currentTarget == null)
            return;
        pos = transform.position;
        Vector3 target = currentTarget.GetPosition();
        _rotateTowards(target);
        _zeroVelocity();
        if (Vector3.Distance(pos, target) > 1)
        {
            Vector3 velocity = Vector3.Normalize(target - pos) * moveSpeed;
            velocity.y = 0f;
            _rb.velocity += velocity;
        }

    }
    #endregion
    #region Debugging
#if UNITY_EDITOR
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 p = pos + Vector3.up;
        Vector3 dir = p + RB.velocity.normalized * 3f;

        Gizmos.DrawLine(p, dir);
    }
#endif
    #endregion
}