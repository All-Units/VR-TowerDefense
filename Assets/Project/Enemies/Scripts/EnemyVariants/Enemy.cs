using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


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
    public float _moveSpeed => enemyStats.MoveSpeed;
    public float _targetTolerance => enemyStats.targetTolerance;
    public float _rotateDamping => enemyStats.rotateDamping;
    public float _attackThreshold => enemyStats.attackThreshold;
    public float _positionTrackTime => enemyStats.PositionTrackTime;
    public int _maxHealth => enemyStats.Health;
    /// <summary>
    /// What type of enemy we are
    /// </summary>
    public EnemyType Type => enemyStats.type;
    [SerializeField] protected Animator animator;
    [SerializeField] public HealthController _hc;
    [SerializeField] protected Rigidbody _RB;
    public Rigidbody RB => _RB;
    [SerializeField] protected Rigidbody ragdollRB;
    [SerializeField] protected CapsuleCollider _Hitbox;
    [SerializeField] protected ParticleSystem _hitEffect;
    [SerializeField] protected Transform _ragdollParent;
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
    /// <summary>
    /// Ensures all components have references (GetComponent)
    /// </summary>
    protected virtual void _InitComponents()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (_hc == null) _hc = GetComponent<HealthController>();
    }
    /// <summary>
    /// Adds all functions to health controller listeners
    /// </summary>
    protected virtual void _InitHealthController()
    {
        _hc.OnDeath += HealthControllerOnOnDeath;
        _hc.OnDeath += OnEnemyDeath;
        _hc.SetMaxHealth(_maxHealth);
        _hc.OnTakeDamage += OnTakeDamage;
    }
    protected virtual void Update()
    {
        //Cache position first
        pos = transform.position;
        _StateMachineUpdate();
    }

    #endregion

    #region StateMachine

    protected virtual void OnEnemySpawn()
    {
        _InitComponents();

        //Initialize health controller
        _InitHealthController();

        spawnTime = Time.realtimeSinceStartup;

        //Disable ragdoll
        _EnableRagdoll(false);

        _SetTarget(nextPoint);
        _SetSpeed(1f);
    }

    protected virtual void OnEnemyDeath()
    {
        animator.enabled = false;
        _EnableRagdoll(true);
        print("ded");  
        //Temp fling ragdoll backwards
        Vector3 dir = transform.forward * -1f + Vector3.up; dir = dir.normalized;
        dir *= enemyStats.RagdollForce;
        //FlingRagdoll(dir);


        Destroy(gameObject, enemyStats.RagdollTime);
        StartCoroutine(_followRagdoll());

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
    /// <summary>
    /// Logic for when the enemy is moving towards goal
    /// </summary>
    protected virtual void _MoveState()
    {
        if (nextPoint == null) return;

        float distance = Utilities.FlatDistance(pos, _target);
        //We've reached our current target
        if (distance <= _targetTolerance)
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
            _targetSelector = SelectNewTarget();
            StartCoroutine(_targetSelector);
            if (currentTarget == null) return;
        }

        //We have a target, how far are they?
        float d = Utilities.FlatDistance(pos, _target);
        d -= _HitboxRadius;
        //We're too far away, move closer and do not attack
        if (d >_attackThreshold)
        {
            _Move();
            return;
        }
        else
        {
            //TO DO  do this!

            /*
            listeningForFootstep = true;
            timeSinceLastAttack = Time.time - lastAttackTime;

            lastAttackTime = Time.time;

            Attacking = true;
            _rb.constraints = RigidbodyConstraints.FreezeAll;
            UpdateSpeed(0f);*/

        }

        
    }
    protected virtual void OnTakeDamage(int currentHealth)
    {
        _hitEffect.Play();
        _GetHit();
        _IsMovementFrozen = true;
        print($"OW!");
    }


    #endregion


    #region StateMachineHelpers
    /// <summary>
    /// Positions, and the Time.time we were in that pos
    /// </summary>
    struct FramePositions
    {
        public FramePositions(float t, Vector3 p) { time = t; position = p; }
        public float time;
        public Vector3 position;
    }
    LinkedList<FramePositions> _posFrames = new LinkedList<FramePositions>();
    /// <summary>
    /// Adds up the flat distance travelled in _posFrames
    /// </summary>
    /// <returns></returns>
    protected float _DistanceTravelled()
    {
        float d = 0;
        if (_posFrames.Count == 0) return d;
        Vector3 lastFrame = _posFrames.First.Value.position;
        foreach (var fp in _posFrames)
        {
            d += fp.position.FlatDistance(lastFrame);
            lastFrame = fp.position;
        }
        return d;
    }
    /// <summary>
    /// Moves and faces the enemy rigidbody towards _target
    /// </summary>
    void _Move()
    {
        //Add current pos to _posFrames
        float t = Time.time;
        _posFrames.AddLast(new FramePositions(t, pos));
        var oldest = _posFrames.First.Value;
        //The oldest frame on record is old enough, pop it
        if (t - oldest.time >= _positionTrackTime)
        {
            _posFrames.RemoveFirst();
        }
        //Update our speed to be  current / max 
        // clamp01
        //Update ANIM speed

        


        //We don't want to rotate rb, but also remove any position constraints
        RB.constraints = RigidbodyConstraints.FreezeRotation;

        //Set anim attacking to false
        _SetAttacking(false);

        //Get normalized vector pointing to target
        Vector3 dir = _target - pos;
        dir.y = 0;
        dir = dir.normalized;

        //Scale velocity
        //Also scale by MAGIC VALUE,
        //because rb.AddForce is smaller than setting rb.velocity directly
        dir *= (_moveSpeed * 30f);
        if (_IsMovementFrozen == false)
            RB.AddForce(dir);
        if (_IsMovementFrozen) { 
            RB.velocity = Vector3.zero;
            //get last - 1 frame (not current) and set our pos to that
            //var lastPos = _posFrames.Last.Previous.Value;
            //if (lastPos.position != pos)
            {
                //transform.position = lastPos.position;
                //print("Manually resetting position to freeze");
            }
            RB.constraints = RigidbodyConstraints.FreezeAll;
        }
        _rotateTowards(_target);

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
            catch (MissingReferenceException e) { continue; }
            catch (NullReferenceException e) { continue; }
            valid.Add(t);
        }

        //update target list
        _targets = valid.ToHashSet();
    }

    protected virtual IEnumerator SelectNewTarget()
    {
        //Do nothing if no towers in range
        if (_targets.Count == 0)
            yield break;
        //Cull all invalid targets
        _CullTargets();
        //Find the closest tower to us
        var closest = _targets.OrderBy(t => Utilities.FlatDistance(t.GetHealthController().transform.position, pos)).FirstOrDefault();
        //Set closest tower as new target
        _target = closest.GetHealthController().transform.position;
        currentTarget = closest;
        _targetSelector = null;
    }
    /// <summary>
    /// Turns on/off every COLLIDER and RIGIDBODY on the ragdoll
    /// </summary>
    /// <param name="enabled">Defaults to turning ragdoll OFF</param>
    protected virtual void _EnableRagdoll(bool enabled = false)
    {
        RB.isKinematic = enabled;
        _Hitbox.isTrigger = enabled;
        _EnableRagdollRBs(enabled);
        _EnableRagdollColliders(enabled);
    }
    /// <summary>
    /// Turns physics on/off for every rigidbody on the ragdoll
    /// </summary>
    /// <param name="enabled"></param>
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
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, _rotateDamping * Time.deltaTime);
    }
    protected virtual IEnumerator _followRagdoll()
    {
        yield break;
        //Unparent and follow ragdoll GO
        _ragdollParent = ragdollRB.transform;
        _ragdollParent.parent = null;
        Destroy(_ragdollParent.gameObject, enemyStats.RagdollTime);
        float t = Time.time;
        while (Time.time - t <= enemyStats.RagdollTime)
        {
            transform.position = ragdollRB.transform.position;
            yield return null;
        }
        
    }

    #endregion

    #region AnimHelpers
    public virtual void Footstep()
    {
    }
    /// <summary>
    /// When the enemy's attack impacts the tower
    /// </summary>
    public virtual void Impact()
    {

        if (currentTarget == null)
        {
            currentTarget = null;
            //attacking = false;
            //_anim.SetTrigger("run");
            return;
        }
        //hitSFXController.PlayClip();
        //currentTarget.GetHealthController().TakeDamage(damage);
    }
    protected void _SetAttacking(bool attacking) { animator.SetBool("IsAttacking", attacking); }
    protected void _SetSpeed(float speed)
    {
        speed = Mathf.Clamp01(speed);
        animator.SetFloat("Speed", speed);
    }
    protected void _GetHit()
    {
        animator.SetTrigger("GetHit");
    }

    public void FinishedGetHitAnim()
    {
        print("Finished playing GetHit anim");
        _IsMovementFrozen = false;
    }
    #endregion

    #region HelperFunctions
    protected void FlingRagdoll(Vector3 force)
    {
        ragdollRB.AddForce(force, ForceMode.Impulse);
    }
    /// <summary>
    /// Updates the current path point we are targeting
    /// </summary>
    /// <param name="target"></param>
    void _SetTarget(PathPoint target)
    {
        if (target == null) return;
        nextPoint = target;
        _target = nextPoint.position;
    }

    #endregion

    private void HealthControllerOnOnDeath()
    {
        OnDeath?.Invoke(this);
    }

    
}