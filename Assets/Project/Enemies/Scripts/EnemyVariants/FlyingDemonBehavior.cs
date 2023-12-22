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
    public override void Impact()
    {
        if (currentTarget == null) return;
        attackSFXController.PlayClip();
        Vector3 target = currentTarget.GetPosition() + Vector3.up * 2f;
        _firePoint.LookAt(target);
        _Fire();
        print("Shooting fireball!");

    }
    #endregion

    #region HelperFunctions

    void _Fire()
    {
        GameObject fireball = Instantiate(_fireballPrefab);
        var projectile = fireball.GetComponent<Projectile>();
        
        fireball.transform.position = _firePoint.position;
        fireball.transform.rotation = _firePoint.rotation;
        projectile.Fire();
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
