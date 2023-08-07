using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class EnemyAnimEvents : MonoBehaviour
{
    /// <summary>
    /// Dirty way of calling
    /// </summary>
    public int HasHit;
    private BasicEnemy enemy;
    
    [SerializeField] AudioClipController _ac;

    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponentInParent<BasicEnemy>();

        var controller = enemy._anim.runtimeAnimatorController;
        foreach (var ssm in ((AnimatorController)controller).layers)
        {
            print($"Root state machine: {ssm.name}");
            foreach (var state in ssm.stateMachine.stateMachines)
            {
                print($"Sub : {state.stateMachine.name} state?");
            }
        }
        var sm = ((AnimatorController)controller).layers[0].stateMachine;
        
        print($"Root state machine: {sm.name}");
        foreach (var state in sm.stateMachines)
        {
            print($"{state.stateMachine.name} state?");
        }
        var states = sm.states;
    }

    private float lastHit;
    

    public void Impact(float f)
    {
        enemy.Impact();
    }

    public void Footstep()
    {
        _ac.PlayClip();
    }
}
