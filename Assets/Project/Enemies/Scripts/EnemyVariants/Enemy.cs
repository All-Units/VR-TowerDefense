using CartoonFX;
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
public abstract class Enemy : MonoBehaviour, IPausable
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
    [SerializeField] protected AudioClipController vocalizationController;
    [SerializeField] protected AudioClipController attackVocalizationController;
    [SerializeField] protected ParticleSystem _hitParticles;
    public float _HitboxRadius => _Hitbox.radius;

    #endregion

    #region InternalVariables
    public Vector3 Pos => pos;


    private IPausableComponents _ipComponents = null;
    public IPausableComponents IPComponents
    {
        get
        {
            if (_ipComponents == null) _ipComponents = this.GetPausableComponents();
            return _ipComponents;
        }
    }

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
    void OnDestroy()
    {
        OnDestroyPausable();
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 p = pos;
        p.y += 5f;
        Vector3 t = _target;
        t.y = p.y;
        Gizmos.DrawLine(p, t);
        Gizmos.DrawSphere(t, .8f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(p, .8f);
    }
    #endregion

    #region StateMachine
    protected virtual void OnEnemySpawn()
    {
        _MoveSpeed = enemyStats.MoveSpeed + Random.Range(-enemyStats.MoveSpeedVariance, enemyStats.MoveSpeedVariance);

        


        EnemyManager.EnemySpawned(this);

        _InitComponents();

        _InitHC();



        spawnTime = Time.realtimeSinceStartup;
        _EnableRagdoll(false);
        _SetSpeed(1f);
        StartCoroutine(_DelayRunAnim());
        StartCoroutine(_Vocalize());
        OnInitPausable();

        
    }
    protected virtual void OnEnemyDie()
    {
        animator.enabled = false;


        RB.isKinematic = true;
        RB.constraints = RigidbodyConstraints.FreezeAll;
        _Hitbox.enabled = false;

        _EnableRagdoll(true);
        gameObject.DestroyAfter(enemyStats.RagdollTime);

        StatusEffectController status = GetComponentInChildren<StatusEffectController>();
        status.PointUpwards();
        status.transform.parent = ragdollRB.transform;
        status.transform.localPosition = new Vector3(0f, -1f, 0f);

        EnemyManager.EnemyKilled(this);

        //_PlayDeathParticles();
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
        int dmg = _lastHealthTotal - currentHealth;
        if (dmg == 0) return;

        ImpactText.ImpactTextAt(_hitParticles.transform.position, dmg.ToString(), ImpactText._ImpactTypes.Damage);
        /*
        float t = (float)dmg / (float)_health;
        float size = Mathf.Lerp(0.6f, 3f, t);
        GameObject particles = Instantiate(Resources.Load<GameObject>("POW"));
        particles.transform.position = _hitParticles.transform.position;
        Vector3 rot = _hitParticles.transform.eulerAngles;
        rot.y += 180f;
        particles.transform.eulerAngles = rot;
        CFXR_ParticleText text = particles.GetComponent<CFXR_ParticleText>();
        if (text != null )
            text.UpdateText($"{dmg}", size);
        ParticleSystem particleSystem = particles.GetComponent<ParticleSystem>();
        if (particleSystem != null)
            particleSystem.Play();

        particles.DestroyAfter(4f);*/
        _lastHealthTotal = currentHealth;
        //_hitParticles.Play();
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
        if (XRPauseMenu.IsPaused) return;
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

        //We're closer to nextnext than current target is, request new target and nextPoint
        if (nextPoint.nextPoint != null && 
            pos.FlatDistance(nextPoint.nextPoint.position) < _target.FlatDistance(nextPoint.nextPoint.position))
        {
            nextPoint = nextPoint.nextPoint;
            _target = nextPoint.GetPoint(_HitboxRadius);
        }
        //If we're closer to NextPoint than Target is, request a new target
        else if (pos.FlatDistance(nextPoint.position) < _target.FlatDistance(nextPoint.position))
        {
            _target = nextPoint.GetPoint(_HitboxRadius);
        }

        float distance = pos.FlatDistance(_target);
        


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

    #region Combat

    public float ApplyResistanceWeakness(List<DamageType> damageType)
    {
        if (enemyStats == null) return 1f;
        return enemyStats.resistancesWeakness.GetModifier(damageType);
    }

        #endregion


    #region StateMachineHelpers
    void _PlayDeathParticles()
    {
        Vector3 pos = _hitParticles.transform.position;
        pos += Vector3.one * 2f;
        string s = $"+ ${enemyStats.KillValue}";
        ImpactText.ImpactTextAt(pos, s, ImpactText._ImpactTypes.Kill, 2f);

        return;
        GameObject particles = Instantiate(Resources.Load<GameObject>("WOW"));
        particles.transform.position = _hitParticles.transform.position;
        particles.transform.Translate(new Vector3(0f, 1f, 0f));
        Vector3 rot = _hitParticles.transform.eulerAngles;
        rot.y += 180f;
        particles.transform.eulerAngles = rot;
        CFXR_ParticleText text = particles.GetComponent<CFXR_ParticleText>();
        if (text != null)
            text.UpdateText($"+ ${enemyStats.KillValue}", 3f);
        ParticleSystem particleSystem = particles.GetComponent<ParticleSystem>();
        if (particleSystem != null)
            particleSystem.Play();

        particles.DestroyAfter(4f);
    }
    IEnumerator _Vocalize()
    {
        if (vocalizationController == null) yield break;
        while (true)
        {
            float t = 0f;
            while (t <= enemyStats.VocalizationCheckRate)
            {
                if (XRPauseMenu.IsPaused == false)
                    t += Time.deltaTime;
                yield return null;
            }
            float roll = Random.value;
            if (healthController.isDead)
                yield break;
            //We rolled within range to vocalize
            if (roll <= enemyStats.VocalizationChance)
            {
                vocalizationController.PlayClip();
            }
        }
    }
    IEnumerator _DelayRunAnim()
    {
        float delay = Random.Range(0, 0.5f);
        animator.Play("Attack");
        yield return null;
        yield return new WaitForSeconds(delay);
        animator.Play("Move");
    }
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
        return _GetClosestTarget();
    }
    protected virtual IEnemyTargetable _GetClosestTarget()
    {
        _CullTargets();
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
            catch (MissingReferenceException e) { continue; }
            catch (NullReferenceException e) { continue; }
            
            
            valid.Add(t);
        }
        _targets = valid.ToHashSet();   
    }

    protected virtual void OnTargetDeath()
    {
        if (_targets.Contains(currentTarget))
            _targets.Remove(currentTarget);
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
            rb.gameObject.layer = 16;
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
            collider.gameObject.layer = 16;
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

        RB.mass *= 200f;
        _MoveSpeed *= 200f;
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
        _lastHealthTotal = _health;
        healthController.SetMaxHealth(_health);
    }
    int _lastHealthTotal;
    public void OnInitPausable()
    {
        this.InitPausable();
    }
    public void OnDestroyPausable()
    {
        this.DestroyPausable();
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

    protected int _lastFootstepFrame = 0;
    public virtual void Footstep()
    {
        if (Time.frameCount - _lastFootstepFrame <= 4) return;
        _lastFootstepFrame = Time.frameCount;
        footstepController.PlayClip();
    }
    protected int _lastAttackFrame = 0;
    public virtual void Impact()
    {
        if (currentTarget == null) return;
        if (Time.frameCount - _lastAttackFrame <= 12) return;
        _lastAttackFrame = Time.frameCount;
        attackSFXController.PlayClip();
        float dmg = _damage;

        if (_IsPowerAttacking)
            dmg *= enemyStats.PowerAttackScalar;
        int damage = (int)dmg;
        currentTarget.GetHealthController().TakeDamage(damage);

    }
    public virtual void AttackVocalization()
    {
        if (attackVocalizationController == null) return;
        float roll = Random.value;
        if (roll <= enemyStats.AttackVocalizationChance)
            attackVocalizationController.PlayClip();
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

    public void OnPause()
    {
        this.BaseOnPause();
    }

    public void OnResume()
    {
        this.BaseOnResume();
    }
    public void KillOOB()
    {
        EnemyManager.EnemyKilled(this);

        Destroy(gameObject);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}