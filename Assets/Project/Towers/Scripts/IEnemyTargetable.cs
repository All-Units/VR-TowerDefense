using UnityEngine;

public interface IEnemyTargetable
{
    HealthController GetHealthController();
    Vector3 GetPosition();
}