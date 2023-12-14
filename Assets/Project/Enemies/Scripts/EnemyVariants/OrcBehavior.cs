using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcBehavior : Enemy
{
    public Vector3 flingDir = new Vector3(1f, 1f, 0f);
    public float RB_Force = 100f;
    [Range(0f, 1f)] public float speed;
    [Range(0f, 1f)] public float AttackStrength;
    public bool IsAttacking;

    public bool Taunt;
    public bool Victory;
    

    // Start is called before the first frame update
    void Start()
    {
       
        //StartCoroutine(_fling());
    }
    IEnumerator _fling()
    {
        yield return new WaitForSeconds(3f); 
        animator.enabled = false;
        print($"Flinging greg");
        float t = Time.time;
        Vector3 dir = flingDir.normalized * RB_Force;
        yield return null;
        ragdollRB.AddForce(dir, ForceMode.Impulse);
        while (Time.time - t <= 0.2f)
        {
            yield return null;
            ragdollRB.AddForce(dir);
            //print($"rb velocity: {ragdollRB.velocity}");
        }
        
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        animator.SetFloat("Speed", speed);
        animator.SetFloat("AttackStrength", AttackStrength);
        animator.SetBool("IsAttacking", IsAttacking);
        if (Taunt)
        {
            animator.SetTrigger("Taunt");
            Taunt = false;
        }
        if (Victory) { animator.SetTrigger("Victory"); Victory = false; }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 dir = pos + RB.velocity.normalized * 3f;
        Gizmos.DrawLine(pos, dir);
        Gizmos.color = Color.green;
        Gizmos.DrawLine (pos, nextPoint.position);
    }

}
