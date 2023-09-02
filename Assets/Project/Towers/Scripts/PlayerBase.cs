using UnityEngine;

[RequireComponent(typeof(HealthController))]
public class PlayerBase : MonoBehaviour, IEnemyTargetable
{
    public HealthController healthController;

    public HealthController GetHealthController()
    {
        return healthController;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}