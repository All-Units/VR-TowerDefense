using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
    public EnemyDTO Stats => enemyStats;
    public float _MoveSpeed;
    public float _TargetTolerance => enemyStats.targetTolerance;
    public float _RotateDamping => enemyStats.rotateDamping;
    public float _attackThreshold => enemyStats.attackThreshold;
    public int _damage => enemyStats.Damage;
    public int _health => enemyStats.Health;
    [HideInInspector]
    public bool _IsAttacking = false;
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
    public Vector3 Pos => pos;
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
    /// <summary>
    /// If the target list contains a given enemy
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool TargetsContains(IEnemyTargetable target) => _targets.Contains(target);
    public void AddTarget(IEnemyTargetable target) => _targets.Add(target);
    public void RemoveTarget(IEnemyTargetable target) => _targets.Remove(target);
    protected IEnumerator _targetSelector;
    protected IEnemyTargetable currentTarget;
    protected bool _IsMovementFrozen = false;
    protected float _lastNeighborCheckTime = 0f;


    protected bool _IsFrozenAfterHit = false;

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


        //Add kill value to currency
        CurrencyManager.GiveToPlayer(enemyStats.KillValue);
    }
    /// <summary>
    /// Invoked when the healthController takes damage
    /// </summary>
    /// <param name="currentHealth">Enemy HP left</param>
    protected virtual void OnEnemyTakeDamage(int currentHealth)
    {
        if (_IsAttacking == false)
            animator.Play("GetHit");
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

    }

    /// <summary>
    /// Called every frame, base enemy state machine logic
    /// </summary>
    protected virtual void _StateMachineUpdate()
    {
        _IsAttacking = false;
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
            _Attack();
    }
    float _lastPowerAttackTime = -1f;




    #endregion




    #region StateMachineHelpers
    protected bool _IsPowerAttacking = false;

    void _UpdatePowerAttackTime()
    {
        if (_lastPowerAttackTime == -1f)
            _lastPowerAttackTime = enemyStats.MinPowerAttackTime;
        //It hasn't been long enough, just add deltaTime and move on
        if (_lastPowerAttackTime < enemyStats.MinPowerAttackTime)
            _lastPowerAttackTime += Time.deltaTime;
        else
        {
            //The time since we reached the threshold
            float t = _lastPowerAttackTime - enemyStats.MinPowerAttackTime;
            //We need to wait more
            if (t < enemyStats.PowerAttackTime)
                _lastPowerAttackTime += Time.deltaTime;
            else
            {
                float r = Random.value;
                //We hit the power attack
                if (r <= enemyStats.PowerAttackChance)
                {
                    _SetAttackStrength(1f);
                    _lastPowerAttackTime = 0f;

                }

                else {
                    _SetAttackStrength(0f);
                    _lastPowerAttackTime = enemyStats.MinPowerAttackTime;
                }

            }
        }
    }
    protected virtual void _Attack()
    {
        _UpdatePowerAttackTime();

        RB.constraints = RigidbodyConstraints.FreezeAll;
        _SetIsAttacking(true);
        _IsAttacking = true;
    }
    protected virtual void _Move()
    {
        _SetIsAttacking(false);
        if (_IsFrozenAfterHit)
        {
            Vector3 velocity = RB.velocity;
            velocity.x = 0f; velocity.z = 0f;
            RB.velocity = velocity;
            return;
        }
        
        RB.constraints = RigidbodyConstraints.FreezeRotation;
        Vector3 dir = _target - pos;
        dir.y = 0;
        dir = dir.normalized;

        //Scale velocity
        dir *= _MoveSpeed;


        RB.AddForce(dir);
        _rotateTowards(_target);

    }
    protected virtual void _EnableRagdoll(bool enabled = false)
    {
        _EnableRagdollRBs(enabled);
        _EnableRagdollColliders(enabled);
    }

    protected virtual void _CompareNeighbors()
    {
        if (Time.time - _lastNeighborCheckTime <= enemyStats.CheckForNeighborsRate) return;
        _lastNeighborCheckTime = Time.time;
        var neighbors = Physics.OverlapSphere(pos, _HitboxRadius + 1.5f);

        Enemy closestNeighbor = this;
        float distance = GetDistanceFromGoal();
        //Iterate over each enemy neighbor
        foreach (var neighbor in neighbors)
        {
            if (neighbor.isTrigger) continue;
            Enemy e = neighbor.GetComponent<Enemy>();
            if (e == null || e == this) { continue; }
            //if (pos.FlatDistance(e.Pos) >= _HitboxRadius + 1.5) continue;
            float d = e.GetDistanceFromGoal();
            if (d < distance)
            {
                distance = d;
                closestNeighbor = e;
            }
        }
        if (closestNeighbor != this)
        {
            _SetPoint(closestNeighbor.nextPoint, closestNeighbor._target);
            //Debug.Log($"{gameObject.name} adopted the target of {closestNeighbor.gameObject.name}", closestNeighbor.gameObject);
        }



    }
    protected virtual void SelectNewTarget()
    {
        if (_targetSelector != null) return;
        _targetSelector = SelectNewTargetRoutine();
        StartCoroutine(_targetSelector);
    }
    protected virtual IEnemyTargetable _GetNextTarget()
    {
        var closest = _targets.OrderBy(t => Utilities.FlatDistance(t.GetHealthController().transform.position, pos)).FirstOrDefault();
        return closest;
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
        var closest = _GetNextTarget();
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
            _ragdollRBs = GetComponentsInChildren<Rigidbody>(true).ToList();

        //Invert because isKinematic = TRUE
        //Means physics DOES NOT affect rb
        foreach (var rb in _ragdollRBs)
        {
            if (rb.gameObject == gameObject) continue;
            rb.isKinematic = !enabled; 
        }
    }
    /// <summary>
    /// Finds all colliders on the ragdoll, and turns them on/off
    /// </summary>
    /// <param name="enabled">Whether to enable the ragdoll cols or not. Defaults to off</param>
    void _EnableRagdollColliders(bool enabled = false)
    {
        
        //Get list of colliders if empty
        if (_ragdollColliders.Count == 0) 
            _ragdollColliders = GetComponentsInChildren<Collider>(true).ToList();

        //Set col.enabled to parameter enabled
        foreach (var collider in _ragdollColliders) 
        { 
            if (collider.gameObject == gameObject) continue; 
            collider.enabled = enabled; 
        }
    }
    /// <summary>
    /// Rotates us towards the given target
    /// </summary>
    /// <param name="pos">our current position</param>
    /// <param name="nextPos">current target's position</param>
    protected virtual void _rotateTowards(Vector3 nextPos)
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

        if (_detectionSphere == null) { _detectionSphere = GetComponentInChildren<SphereCollider>(); }
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
    public float GetDistanceFromGoal()
    {
        if (nextPoint == null)
            return 0f;
        return nextPoint.DistanceToGoalFrom(pos);
    }
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
    protected virtual void _SetPoint(PathPoint target, Vector3 pos)
    {
        if (target == null) return;
        nextPoint = target;
        _target = pos;
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
        if (isAttacking && _IsAttacking == false)
        {
            animator.Play("Attack");
        }
        
    }
    protected virtual void _SetAttackStrength(float strength)
    {
        
        strength = Mathf.Clamp01(strength);
        _IsPowerAttacking = strength == 1;
        animator.SetFloat("AttackStrength", strength);
    }
    public virtual void Footstep()
    {
        footstepController.PlayClip();
    }
    public virtual void Impact()
    {
        if (currentTarget == null) return;
        attackSFXController.PlayClip();
        float dmg = _damage;
        if (_IsPowerAttacking)
            dmg *= enemyStats.PowerAttackScalar;
        currentTarget.GetHealthController().TakeDamage((int)dmg);

    }
    public virtual void StartGetHit()
    {
        //_IsFrozenAfterHit = true;
    }
    public virtual void EndGetHit()
    {
        _IsFrozenAfterHit = false;
    }
    #endregion

    private void HealthControllerOnOnDeath()
    {
        OnDeath?.Invoke(this);
    }

    
}