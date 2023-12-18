using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

/// <summary>
/// Base enemy logic
/// </summary>
[RequireComponent(typeof(HealthController))]
public abstract class Enemy : MonoBehaviour
{
    #region PublicVariables
    public float spawnTime { get; private set; }
    public static Action<Enemy> OnDeath;



    [Header("Component references")]
    public PathPoint nextPoint;
    [SerializeField] protected EnemyDTO enemyStats;
    [HideInInspector]
    public float _MoveSpeed;
    public float _TargetTolerance => enemyStats.targetTolerance;
    public float _RotateDamping => enemyStats.rotateDamping;
    public float _attackThreshold => enemyStats.attackThreshold;
    public int _damage => enemyStats.Damage;
    public int _health => enemyStats.Health;
    /// <summary>
    /// What type of enemy we are
    /// </summary>
    public EnemyType Type => enemyStats.type;
    [SerializeField] protected Animator animator;
    [SerializeField] public HealthController healthController;
    [SerializeField] protected Rigidbody _RB;
    public Rigidbody RB => _RB;
    [SerializeField] protected Rigidbody ragdollRB;
    [SerializeField] protected CapsuleCollider _Hitbox;
    [SerializeField] protected SphereCollider _detectionSphere;
    [SerializeField] protected AudioClipController footstepController;
    [SerializeField] protected AudioClipController attackSFXController;
    [SerializeField] protected ParticleSystem _hitParticles;
    public float _HitboxRadius => _Hitbox.radius;

    #endregion


    #region InternalVariables

    /// <summary>
    /// Updated every frame, caches transform.position
    /// </summary>
    protected Vector3 pos;

    /// <summary>
    /// The current world pos that this Enemy is trying to reach
    /// </summary>
    protected Vector3 _target;

    List<Collider> _ragdollColliders = new List<Collider>();
    List<Rigidbody> _ragdollRBs = new List<Rigidbody>();
    /// <summary>
    /// The current list of towers in range
    /// </summary>
    protected HashSet<IEnemyTargetable> _targets = new HashSet<IEnemyTargetable>();
    protected IEnumerator _targetSelector;
    protected IEnemyTargetable currentTarget;
    protected bool _IsMovementFrozen = false;

    #endregion

    #region UnityEvents
    // Initialize Enemy logic
    protected virtual void Awake()
    {
        OnEnemySpawn();
    }
    
    protected virtual void Update()
    {
        //Cache position first
        pos = transform.position;
        _StateMachineUpdate();
    }
    private void OnTriggerEnter(Collider other)
    {
        //If we don't have this enemy in our target list
        if (other.TryGetComponent(out IEnemyTargetable enemy) 
            && _targets.Contains(enemy) == false)
        {
            _targets.Add(enemy);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IEnemyTargetable enemy)
            && _targets.Contains(enemy))
        {
            _targets.Remove(enemy);
        }
    }
    #endregion

    #region StateMachine
    protected virtual void OnEnemySpawn()
    {
        _MoveSpeed = enemyStats.MoveSpeed + Random.Range(-enemyStats.MoveSpeedVariance, enemyStats.MoveSpeedVariance);


        if (EnemyManager.Enemies.Contains(this) == false)
            EnemyManager.Enemies.Add(this);
        EnemyManager.EnemyCount++;
        EnemyManager.GregSpawned();

        _InitComponents();

        _InitHC();

        
        spawnTime = Time.realtimeSinceStartup;
        _EnableRagdoll(false);
        _SetSpeed(1f);
    }
    protected virtual void OnEnemyDie()
    {
        animator.enabled = false;


        RB.isKinematic = true;
        _Hitbox.enabled = false;

        _EnableRagdoll(true);
        Destroy(gameObject, enemyStats.RagdollTime);

        if (EnemyManager.Enemies.Contains(this))
            EnemyManager.Enemies.Remove(this);
        EnemyManager.EnemyCount--;
        EnemyManager.GregKilled();
    }
    /// <summary>
    /// Invoked when the healthController takes damage
    /// </summary>
    /// <param name="currentHealth">Enemy HP left</param>
    protected virtual void OnEnemyTakeDamage(int currentHealth)
    {
        _hitParticles.Play();
    }

    protected virtual void OnEnemyTakeDamageFrom(int currentHealth, Vector3 source)
    {
        //Do nothing if not dead
        if (healthController.isDead == false) return;


        animator.enabled = false;


        RB.isKinematic = true;
        _Hitbox.enabled = false;

        _EnableRagdoll(true);

        Vector3 dir = (pos - source).normalized; dir.y = 0f;
        dir += Vector3.up; dir = dir.normalized;

        //Scale RB force by magic number to make more readable

        dir *= enemyStats.RagdollForce * 8;
        ragdollRB.AddForce(dir, ForceMode.Impulse);
        print($"Flinging ragdoll in dir {dir.normalized}, magnitude {dir.magnitude}");

    }

    /// <summary>
    /// Called every frame, base enemy state machine logic
    /// </summary>
    protected virtual void _StateMachineUpdate()
    {
        //There is at least one target in range
        if (_targets.Count > 0)
        {
            _AttackState();
            return;
        }

        _MoveState();
    }
    private void _MoveState()
    {
        if (nextPoint == null) return;
        _CompareNeighbors();
        
        float distance = Utilities.FlatDistance(pos, _target);
        //We've reached our current target
        if (distance <= _TargetTolerance)
        {
            //Get the next target
            nextPoint = nextPoint.Next;
            //We've reached the Castle
            if (nextPoint == null)
            {
                print($"Reached end");
                //EnemyVictory();
                return;
            }
            else
            {
                _target = nextPoint.GetPoint(_HitboxRadius);
            }
        }
        _Move();

    }
    /// <summary>
    /// Logic for when the enemy has a tower to attack
    /// </summary>
    protected virtual void _AttackState()
    {
        //If we don't have a target, choose a new one
        if (currentTarget == null && _targets.Count > 0)
        {
            if (_targetSelector != null) return;
            SelectNewTarget();
            if (currentTarget == null) return;
        }

        //We have a target, how far are they?
        float d = Utilities.FlatDistance(pos, _target);
        d -= _HitboxRadius;
        //We're too far away, move closer and do not attack
        if (d > _attackThreshold)
        {
            _Move();
            return;
        }
        else
        {
            RB.constraints = RigidbodyConstraints.FreezeAll;
            _SetIsAttacking(true);
            

        }


    }




    #endregion




    #region StateMachineHelpers
    void _Move()
    {
        _SetIsAttacking(false);
        //UpdateSpeed(1f);
        //Do not move if we are listening for a footstep
        //if (listeningForFootstep)
        //    return;
        RB.constraints = RigidbodyConstraints.FreezeRotation;
        Vector3 dir = _target - pos;
        dir.y = 0;
        dir = dir.normalized;

        //Scale velocity
        dir *= _MoveSpeed;

        //Set our velocity in the horizontal plane
        Vector3 velocity = RB.velocity;

        velocity.x = dir.x; velocity.z = dir.z;
        velocity.y = 0f;
        RB.AddForce(velocity);
        //RB.velocity = velocity;
        _rotateTowards(_target);

    }
    protected virtual void _EnableRagdoll(bool enabled = false)
    {
        _EnableRagdollRBs(enabled);
        _EnableRagdollColliders(enabled);
    }
    void _CompareNeighbors()
    {
        //if (Time.time - _lastNeighborCheckTime <= enemyStats.CheckForNeighborsRate) return;
        //var neighbors = Physics.OverlapSphere(pos, _HitboxRadius + 0.5f);//.SphereCast(pos, _HitboxRadius + 0.5f, transform.forward, out RaycastHit hit);

    }
    protected virtual void SelectNewTarget()
    {
        if (_targetSelector != null) return;
        _targetSelector = SelectNewTargetRoutine();
        StartCoroutine(_targetSelector);
    }
    protected virtual IEnumerator SelectNewTargetRoutine()
    {
        currentTarget = null;
        //Wait a frame
        yield return null;
        //Cull all invalid targets
        _CullTargets();

        //Do nothing if no towers in range
        if (_targets.Count == 0)
        {
            if (nextPoint.nextPoint != null)
            {
                _SetTarget(nextPoint.nextPoint);
            }
            else
                _SetTarget(nextPoint);
            yield break;
        }


        //Find the closest tower to us
        var closest = _targets.OrderBy(t => Utilities.FlatDistance(t.GetHealthController().transform.position, pos)).FirstOrDefault();
        //Set closest tower as new target
        _target = closest.GetPosition();
        //_target = closest.GetHealthController().transform.position;
        currentTarget = closest;

        currentTarget.GetHealthController().OnDeath += OnTargetDeath;

        _targetSelector = null;
    }
    /// <summary>
    /// Checks current target list, removes all missing / null refs
    /// </summary>
    protected virtual void _CullTargets()
    {
        var copy = _targets.ToArray();
        var valid = new List<IEnemyTargetable>();
        //Makes a deep copy of all tested and valid targets
        for (int i = 0; i < copy.Count(); i++)
        {
            var t = copy[i];
            try { var h = t.GetHealthController(); var x = h.transform.position; }
            catch (NullReferenceException e) { continue; }
            catch (MissingReferenceException e) { continue; }
            
            valid.Add(t);
        }
        _targets = valid.ToHashSet();   
    }

    protected virtual void OnTargetDeath()
    {
        currentTarget = null;
    }

    void _EnableRagdollRBs(bool enabled = false)
    {
        if (_ragdollRBs.Count == 0) 
            _ragdollRBs = ragdollRB.GetComponentsInChildren<Rigidbody>(true).ToList();

        //Invert because isKinematic = TRUE
        //Means physics DOES NOT affect rb
        foreach (var rb in _ragdollRBs)
            rb.isKinematic = !enabled;
    }
    /// <summary>
    /// Finds all colliders on the ragdoll, and turns them on/off
    /// </summary>
    /// <param name="enabled">Whether to enable the ragdoll cols or not. Defaults to off</param>
    void _EnableRagdollColliders(bool enabled = false)
    {
        //Get list of colliders if empty
        if (_ragdollColliders.Count == 0) 
            _ragdollColliders = ragdollRB.GetComponentsInChildren<Collider>(true).ToList();

        //Set col.enabled to parameter enabled
        foreach (var collider in _ragdollColliders) 
            { collider.enabled = enabled; }
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
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, _RotateDamping * Time.deltaTime);
    }

    protected virtual void _InitComponents()
    {
        //Ensure all components have references
        if (animator == null) animator = GetComponent<Animator>();
        if (healthController == null) healthController = GetComponent<HealthController>();

        if (_detectionSphere == null) { _detectionSphere = GetComponent<SphereCollider>(); }
        float range = Random.Range(enemyStats.MinRange, enemyStats.MaxRange);
        _detectionSphere.radius = range;
    }
    /// <summary>
    /// Initialize health controller
    /// </summary>
    protected virtual void _InitHC()
    {
        healthController.OnDeath += HealthControllerOnOnDeath;
        healthController.OnDeath += OnEnemyDie;
        healthController.OnTakeDamage += OnEnemyTakeDamage;
        healthController.OnTakeDamageFrom += OnEnemyTakeDamageFrom;

        healthController.SetMaxHealth(_health);
    }

    #endregion

    #region HelperFunctions
    /// <summary>
    /// Updates the current path point we are targeting
    /// </summary>
    /// <param name="target"></param>
    public void _SetTarget(PathPoint target)
    {
        if (target == null) return;
        nextPoint = target;
        _target = nextPoint.GetPoint(_HitboxRadius);
    }
    

    #endregion

    #region AnimHelpers
    protected void _SetSpeed(float speed)
    {
        speed = Mathf.Clamp01(speed);
        animator.SetFloat("Speed", speed);
    }
    protected virtual void _SetIsAttacking(bool isAttacking)
    {
        animator.SetBool("IsAttacking", isAttacking);
    }
    public virtual void Footstep()
    {
        footstepController.PlayClip();
    }
    public virtual void Impact()
    {
        if (currentTarget == null) return;
        attackSFXController.PlayClip();
        currentTarget.GetHealthController().TakeDamage(_damage);
        print("thud, from the chud");

    }
    #endregion

    private void HealthControllerOnOnDeath()
    {
        OnDeath?.Invoke(this);
    }

    
}