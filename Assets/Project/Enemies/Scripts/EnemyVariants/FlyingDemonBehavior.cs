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

    [SerializeField] private float _fireRate = 1f;


    

    #endregion


    #region EnemyOverrides
    protected override void Update()
    {
        base.Update();

    }
    protected override void OnEnemySpawn()
    {
        base.OnEnemySpawn();
        _startConstraints = RB.constraints;
        FlightHeight += (int)Random.Range(HeightVariance * -1, HeightVariance);

    }



    protected override void _Move()
    {
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
        catch (MissingReferenceException e) { currentTarget = null; return; }
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
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, _target);
    }
    #endregion
}
