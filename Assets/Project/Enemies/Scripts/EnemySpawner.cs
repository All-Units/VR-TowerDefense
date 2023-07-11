using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField]
    private SpawnPoint _spawnPoint;
    [SerializeField]
    private Transform enemyParent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
        {
            SpawnEnemy();
        }
    }

    public void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, enemyParent);
        enemy.transform.position = _spawnPoint.transform.position;
        enemy.GetComponent<BasicEnemy>().nextWaypoint = _spawnPoint;
    }
}
