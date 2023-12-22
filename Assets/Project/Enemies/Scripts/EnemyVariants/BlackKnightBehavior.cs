using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlackKnightBehavior : Enemy
{
    protected override IEnemyTargetable _GetNextTarget()
    {
        var strongest = _targets.OrderBy(t => t.GetHealthController().CurrentHealth).LastOrDefault();
        return strongest;
    }
}
