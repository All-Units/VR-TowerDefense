using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcBehavior : Enemy
{
    [Header("\nDebug variables")]
    public Vector3 rb_Velocity;
    public float distanceTravelled;

    // Start is called before the first frame update
    void Start()
    {
        
        _SetSpeed(1f);
        //StartCoroutine(_fling());
    }
    

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        rb_Velocity = RB.velocity;
        distanceTravelled = _DistanceTravelled();
        /*
        animator.SetFloat("Speed", speed);
        animator.SetFloat("AttackStrength", AttackStrength);
        animator.SetBool("IsAttacking", IsAttacking);
        if (Taunt)
        {
            animator.SetTrigger("Taunt");
            Taunt = false;
        }
        if (Victory) { animator.SetTrigger("Victory"); Victory = false; }*/
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = (_IsMovementFrozen) ? Color.green : Color.red;
        Gizmos.DrawSphere(pos + Vector3.up * 6, 1f);
        
        return;
        Gizmos.color = Color.cyan;
        Vector3 dir = pos + RB.velocity.normalized * 3f;
        Gizmos.DrawLine(pos, dir);
        Gizmos.color = Color.green;
        Gizmos.DrawLine (pos, nextPoint.position);
    }

}
