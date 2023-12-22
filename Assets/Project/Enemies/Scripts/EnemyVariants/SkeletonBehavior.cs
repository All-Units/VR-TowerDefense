using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkeletonBehavior : Enemy
{
    protected override IEnemyTargetable _GetNextTarget()
    {
        var weakest = _targets.OrderBy(t => t.GetHealthController().CurrentHealth).FirstOrDefault();
        return weakest;
    }
}
