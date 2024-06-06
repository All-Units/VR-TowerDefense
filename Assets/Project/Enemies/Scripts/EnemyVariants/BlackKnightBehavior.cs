using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlackKnightBehavior : Enemy
{
    float _lastFootstepTime = 0f;
    public override void Footstep()
    {
        _lastFootstepTime = Time.time;
        base.Footstep();
    }
    public override void Impact()
    {
        //Do nothing if it's been less than n seconds since last footstep
        if (Time.time - _lastFootstepTime <= 0.4f)
            return;

        float dmg = _damage;

        if (_IsPowerAttacking)
            dmg *= enemyStats.PowerAttackScalar;
        int damage = (int)dmg;
        //string s = $"BK did {damage} dmg to {currentTarget.GetHealthController().gameObject.name}";
        
        base.Impact();
        //Debug.Log(s, currentTarget.GetHealthController());

        
    }
    protected override void _AttackState()
    {
        //Do no special logic if we don't have a target
        if (currentTarget == null)
        {
            base._AttackState();
            return;
        }
        var closest = _GetClosestTarget();
        
        //If there is a closer target than our current target, choose that one
        if (closest != currentTarget)
        {
            if (_targetSelector != null) return;
            SelectNewTarget();
            if (currentTarget == null) return;
        }

        base._AttackState();
    }
    protected override void OnEnemyTakeDamage(int currentHealth)
    {
        //Only play hit if No current target OR distance MORE THAN threshold * 2
        bool _playHit = _IsAttacking == false;
        _playHit = (currentTarget == null || pos.FlatDistance(_target) >= enemyStats.attackThreshold * 2f);
        if (_playHit)
            base.OnEnemyTakeDamage(currentHealth);
    }
    public GameObject CurrentTarget;
    protected override IEnemyTargetable _GetNextTarget()
    {
        var closest = base._GetNextTarget();
        if (closest != null) CurrentTarget = closest.GetHealthController().gameObject;
        return closest;
    }
}
