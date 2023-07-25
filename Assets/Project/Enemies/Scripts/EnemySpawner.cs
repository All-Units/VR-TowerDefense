using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField]
    private SpawnPoint _spawnPoint;
    [SerializeField]
    private Transform enemyParent;

    public float enemySpawnDelay = 1f;

    [SerializeField] private TextAsset levelCSV;

    private static List<EnemySpawner> _spawners = new List<EnemySpawner>();

    private static IEnumerator waveLoop = null;
    // Start is called before the first frame update
    void Start()
    {
        parseCSV();
        //SpawnEnemy(enemyPrefabs.GetRandom());
        if (waveLoop == null)
        {
            print("Spawned one (and only one) spawner!");
            waveLoop = WaveLoop();
            StartCoroutine(waveLoop);
        }
        
        _spawners.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
        {
            SpawnEnemy(enemyPrefabs.GetRandom());
        }
    }

    public void SpawnEnemy(GameObject enemyPrefab)
    {
        EnemySpawner spawner = _spawners.GetRandom();
        GameObject enemy = Instantiate(enemyPrefab, spawner.enemyParent);
        enemy.transform.position = spawner._spawnPoint.transform.position;
        enemy.GetComponent<BasicEnemy>().nextWaypoint = spawner._spawnPoint;
    }
    [SerializeField]
    private List<GameObject> orderedPrefabs = new List<GameObject>();
    private List<List<int>> waveTotals = new List<List<int>>();
    void parseCSV()
    {
        string[] lines = levelCSV.text.Split("\n");
        string[] enemies = lines[0].Split(",");
        foreach (string enemy in enemies)
        {
            GameObject e = enemyPrefabs.Find(x => x.name.ToLower().Trim() == enemy.ToLower().Trim());
            orderedPrefabs.Add(e);
        }
        bool first = true;
        foreach (var line in lines)
        {
            print($"Parsed line {line}");
            if (first)
            {
                first = false;
                continue;
            }

            string[] split = line.Split(",");
            List<int> count = new List<int>();
            foreach (string i in split)
            {
                int j = int.Parse(i);
                print($"{j}");
                count.Add(j);
            }
                
            waveTotals.Add(count);

        }
    }

    private int wave_i = 0;
    IEnumerator WaveLoop()
    {
        yield return new WaitForEndOfFrame();
        List<int> wave = waveTotals[wave_i];
        List<int> available = new List<int>();
        for (int i = 0; i < orderedPrefabs.Count; i++)
        {
            if (wave[i] != 0)
                available.Add(i);
        }

        while (available.Count != 0)
        {
            //Choose a random index from available
            int i = available.GetRandom();
            //Subtract one 
            wave[i] -= 1;
            int count = wave[i];
            if (count == 0)
                available.Remove(i);
            GameObject prefab = orderedPrefabs[i];
            SpawnEnemy(prefab);
            yield return new WaitForSeconds(enemySpawnDelay);

        }
        print("Finished wave");
        yield break;
        do
        {
            //Choose a random one of the enemies in our wave to spawn
            int i = Random.Range(0, orderedPrefabs.Count - 1);
            //If there are none left to spawn, choose again
            while (wave[i] == 0)
                i = Random.Range(0, orderedPrefabs.Count - 1);
        } while (enemyParent.childCount > 0);
        yield break;
    }
}
