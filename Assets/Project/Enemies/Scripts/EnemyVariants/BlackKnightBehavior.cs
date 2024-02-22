using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlackKnightBehavior : Enemy
{
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
