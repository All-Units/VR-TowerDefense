using System;
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
    public float _MoveSpeed => enemyStats.MoveSpeed;
    public float _TargetTolerance => enemyStats.targetTolerance;
    public float _RotateDamping => enemyStats.rotateDamping;
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


    #endregion

    #region UnityEvents

    // Initialize Enemy logic
    protected virtual void Awake()
    {
        _InitComponents();
        healthController.OnDeath += HealthControllerOnOnDeath;
        spawnTime = Time.realtimeSinceStartup;
        _EnableRagdoll();

        _SetTarget(nextPoint);

    }
    protected virtual void _InitComponents()
    {
        //Ensure all components have references
        if (animator == null) animator = GetComponent<Animator>();
        if (healthController == null) healthController = GetComponent<HealthController>();
    }
    protected virtual void Update()
    {
        //Cache position first
        pos = transform.position;
        _StateMachineUpdate();
    }

    #endregion

    #region StateMachine
    /// <summary>
    /// Called every frame, base enemy state machine logic
    /// </summary>
    protected virtual void _StateMachineUpdate()
    {
        _MoveState();
    }
    private void _MoveState()
    {
        if (nextPoint == null) return;

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




    #endregion
    void _Move()
    {
        //UpdateSpeed(1f);
        //Do not move if we are listening for a footstep
        //if (listeningForFootstep)
        //    return;
        //_rb.constraints = RigidbodyConstraints.FreezeRotation;
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

    #region StateMachineHelpers
    protected virtual void _EnableRagdoll(bool enabled = false)
    {
        _EnableRagdollRBs(enabled);
        _EnableRagdollColliders(enabled);
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

    #endregion

    #region HelperFunctions
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

    public virtual void Footstep()
    {
    }
}