using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcBehavior : Enemy
{

    [Header("Debug vars")]
    public Vector3 rb_velocity;
    

    // Start is called before the first frame update
    void Start()
    {
       
        //StartCoroutine(_fling());
    }
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
        rb_velocity = ragdollRB.velocity;
        
    }
    private void OnDrawGizmosSelected()
    {
        //Gizmos.color = Color.cyan;
        //Vector3 dir = pos + RB.velocity.normalized * 3f;
        //Gizmos.DrawLine(pos, );
        Gizmos.color = Color.green;
        Gizmos.DrawLine (pos, _target + Vector3.up);
    }

}
