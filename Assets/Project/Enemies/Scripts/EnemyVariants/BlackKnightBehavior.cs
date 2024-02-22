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
        base.Impact();
    }
    protected override void OnEnemyTakeDamage(int currentHealth)
    {
        //Only play hit if No current target OR distance MORE THAN threshold * 2
        bool _playHit = _IsAttacking == false;
        _playHit = (currentTarget == null || pos.FlatDistance(_target) >= enemyStats.attackThreshold * 2f);
        if (_playHit)
            base.OnEnemyTakeDamage(currentHealth);
        //_hitParticles.Play();
    }
    protected override IEnemyTargetable _GetNextTarget()
    {
        var strongest = _targets.OrderBy(t => t.GetHealthController().CurrentHealth).LastOrDefault();
        return strongest;
    }
}
