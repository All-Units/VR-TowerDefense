using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcBehavior : Enemy
{

    [Header("Debug vars")]
    public Vector3 ragdoll_velocity;
    public Vector3 rb_velocity;

    public int _TargetsInRange;
    public float _distanceToTarget;

    
    
    IEnumerator _fling()
    {
        yield return new WaitForSeconds(3f);
        
        animator.enabled = false;
        

        RB.isKinematic = true;
        _Hitbox.enabled = false;
        
        _EnableRagdoll(true);
        yield return null;

        Vector3 dir = transform.forward * -1f + Vector3.up; dir = dir.normalized;

        dir *= enemyStats.RagdollForce;
        ragdollRB.AddForce(dir, ForceMode.Impulse);
        print("Flinging ragdoll");


    }

    // Update is called once per frame
    protected override void Update()
    {
        
        base.Update();
        ragdoll_velocity = ragdollRB.velocity;
        rb_velocity = RB.velocity;
        _TargetsInRange = _targets.Count;
        _distanceToTarget = Utilities.FlatDistance(pos, _target);
        _distanceToTarget -= _HitboxRadius;

    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos, _HitboxRadius + 1.5f);

        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, _target);

    }

}
