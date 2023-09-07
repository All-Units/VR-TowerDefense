using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour, IEnemyTargetable
{
    [SerializeField] private bool isFinalGate = false;

    public void Die()
    {
        //base.Die();
        //When the final gate falls, the castle falls
        if (isFinalGate)
        {
            GameStateManager.LoseGame();
        }
    }

    public HealthController GetHealthController()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetPosition()
    {
        throw new System.NotImplementedException();
    }
}
