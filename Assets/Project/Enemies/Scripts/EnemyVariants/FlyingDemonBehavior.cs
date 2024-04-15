using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingDemonBehavior : Enemy
{
    #region PublicVariables
    [Header("Demon Movement Variables")]
    RigidbodyConstraints _startConstraints;
    public int FlightHeight = 5;
    public float HeightVariance = 2f;
    [SerializeField] LayerMask _groundLayer;


    [Header("Demon Attack Variables")]
    [SerializeField] private GameObject _fireballPrefab;
    [SerializeField] private Transform _firePoint;





    #endregion


    #region EnemyOverrides
    public GameObject DebugTarget;
    public bool CurrentTargetIsNull;
    protected override void Update()
    {
        if (currentTarget != null)
        {
            try { currentTarget.GetPosition(); currentTarget.GetHealthController(); }
            catch (MissingReferenceException e) { currentTarget = null; SelectNewTarget(); }
        }
        
        base.Update();
        if (currentTarget != null)
            DebugTarget = currentTarget.GetHealthController().gameObject;
        CurrentTargetIsNull = (currentTarget == null);

    }
    protected override void OnEnemySpawn()
    {
        base.OnEnemySpawn();
        _startConstraints = RB.constraints;
        FlightHeight += (int)Random.Range(HeightVariance * -1, HeightVariance);

    }
    /// <summary>
    /// If there is a line of sight from the fire point to the target
    /// </summary>
    /// <returns></returns>
    bool _HasShot()
    {
        int mask = LayerMask.GetMask("Ground");
        Vector3 dir = pos.DirectionTo(_target);
        float distance = Vector3.Distance(pos, _target) - 1f;
        if (Physics.Raycast(pos, dir, distance, mask))
        {
            Debug.Log($"There is ground b/w us and target, returning false {gameObject.name}", gameObject);
            return false;
        }
        return true;
    }

    protected override void _AttackState()
    {
        
        //If we don't have a target, choose a new one
        if (currentTarget == null && _targets.Count > 0)
        {
            if (_targetSelector != null) return;
            SelectNewTarget();
            if (currentTarget == null) return;
        }
        //If we don't have a shot, move closer
        if (_HasShot() == false)
        {
            _Move();
            return;
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

    protected override void _Move()
    {
        if (healthController.isDead) return;
        _SetIsAttacking(false);
        

        RB.constraints = _startConstraints;
        Vector3 dir = _target - pos;
        dir.y = 0;
        dir = dir.normalized;

        //Scale velocity
        dir *= _MoveSpeed;

        //Move horizontally
        RB.AddForce(dir);
        _rotateTowards(_target);


        //Set our elevation
        _SetHeight();
    }
    float _timeSinceLastAttack = 0f;
    Vector3 targetPosition => currentTarget.GetPosition() + Vector3.up * 2f;
    public override void Impact()
    {
        if (currentTarget == null || currentTarget == null) return;
        if (Time.frameCount - _lastAttackFrame <= 20) return;
        _lastAttackFrame = Time.frameCount;
        try { currentTarget.GetPosition(); }
        catch (MissingReferenceException e) { currentTarget = null; SelectNewTarget(); return; }
        if (pos.FlatDistance(targetPosition) >= enemyStats.attackThreshold + 3f) {return;  }
        attackSFXController.PlayClip();
        _firePoint.LookAt(targetPosition);
        _Fire();
    }
    #endregion

    #region HelperFunctions

    void _Fire()
    {
        GameObject fireball = Instantiate(_fireballPrefab);
        var projectile = fireball.GetComponent<Projectile>();
        var aoe = projectile.GetComponent<AOEProjectile>();
        if (aoe)
            aoe.TargetLayer = "Tower";
        fireball.transform.position = _firePoint.position;
        fireball.transform.rotation = _firePoint.rotation;
        fireball.transform.LookAt(targetPosition);
        projectile.Fire();
        projectile.damage = enemyStats.Damage;
        Destroy(fireball, 10f);
    }

    void _SetHeight()
    {
        Vector3 p = pos + Vector3.up * 7f; 
        if (Physics.Raycast(p, Vector3.down, out RaycastHit hit, Mathf.Infinity, _groundLayer))
        {
            float height = hit.point.y + FlightHeight;
            p = transform.position;
            p.y = height;
            transform.position = p;
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (_HasShot() == false)
            Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, _target);
    }
#endif
#endregion
}
