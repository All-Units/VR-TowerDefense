using UnityEngine;

public class EnemyTestSpawner : MonoBehaviour
{
    public BasicEnemy prefab;
    private BasicEnemy currentEnemy;
    
    // Start is called before the first frame update
    private void Start()
    {
        SpawnNewEnemy();
    }

    private void SpawnNewEnemy()
    {
        currentEnemy = Instantiate(prefab, transform);
        currentEnemy._hc.OnDeath += HealthControllerOnOnDeath;
        currentEnemy.reachedEnd = true;
    }

    private void HealthControllerOnOnDeath()
    {
        Invoke(nameof(SpawnNewEnemy), 1f);
    }
}
