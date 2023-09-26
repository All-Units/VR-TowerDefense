using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
    [Header("Wave variables")]
    public float enemySpawnDelay = 1f;
    [Tooltip("The number of seconds between rounds")]
    [SerializeField] private int waveDelay = 20;
    [SerializeField] private List<GameObject> enemyPrefabs;

    [SerializeField] private GameObject nextRoundCounterPanel;

    [SerializeField] private TextAsset levelCSV;

    private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
    private WaveCounterDisplay _counterDisplay;

    public Dictionary<BasicEnemy, Transform> enemies = new Dictionary<BasicEnemy, Transform>();

    public static EnemySpawner instance;
    public bool IsStressTest = false;
    [SerializeField] private float firstRoundDelay = 5f;


    public static UnityEvent OnRoundStarted = new UnityEvent();
    public static UnityEvent OnRoundEnded = new UnityEvent();

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        parseCSV();
        _spawnPoints = GetComponentsInChildren<SpawnPoint>().ToList();
        _counterDisplay = GetComponent<WaveCounterDisplay>();
        StartCoroutine(WaveLoop());
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
        {
            SpawnEnemy(enemyPrefabs.GetRandom());
        }
        UpdateAllEnemyPositions();
    }

    void UpdateAllEnemyPositions()
    {
        foreach (var e in enemies)
        {
            var enemy = e.Key;
            var head = e.Value;
            head.localPosition = enemy.transform.position * 0.02f;
            Vector3 rot = head.localEulerAngles;
            rot.y = enemy.transform.localEulerAngles.y;
            head.localEulerAngles = rot;
        }
    }

    public static void SpawnRandom()
    {
        instance.SpawnEnemy(instance.enemyPrefabs.GetRandom());
    }

    public void SpawnEnemy(GameObject enemyPrefab)
    {
        SpawnPoint point = _spawnPoints.GetRandom();
        GameObject enemy = Instantiate(enemyPrefab, point.enemyParent);
        enemy.transform.position = point.transform.position;
        var e = enemy.GetComponent<BasicEnemy>();
        SpawnHead(e);
        e.nextWaypoint = point;
    }

    void SpawnHead(BasicEnemy enemy)
    {
        GameObject head = Instantiate(enemy.headPrefab, Minimap.Zero);
        enemies.Add(enemy, head.transform);
        head.transform.localPosition = enemy.transform.position * 0.02f;
    }

    public static void RemoveEnemy(BasicEnemy enemy)
    {
        if (instance.enemies.ContainsKey(enemy))
        {
            Destroy(instance.enemies[enemy].gameObject);
            instance.enemies.Remove(enemy);
        }
    }
    [SerializeField]
    private List<GameObject> orderedPrefabs = new List<GameObject>();
    private List<List<int>> waveTotals = new List<List<int>>();
    /// <summary>
    /// Loads the given CSV into a readable wave format, ie list(s) of ints
    /// </summary>
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
            if (first)
            {
                first = false;
                continue;
            }
            if (line.Trim() == "")
                continue;
            

            string[] split = line.Split(",");
            List<int> count = new List<int>();
            foreach (string i in split)
            {
                int j = int.Parse(i);
                count.Add(j);
            }
                
            waveTotals.Add(count);

        }
    }

    public static int CurrentWave => instance.wave_i + 1;
    public static int MaxWaves => instance.waveTotals.Count;
    private int wave_i = 0;

    void _win()
    {
        GameStateManager.WinGame();
    }

    private bool run = false;
    IEnumerator WaveLoop()
    {
        if (IsStressTest) yield break;
        if (run == false)
        {
            yield return new WaitForSeconds(0.2f);
            run = true;
            _counterDisplay.SetPanelVisibility(true);
            for (int i = 0; i < (int)firstRoundDelay; i++)
            {
                _counterDisplay.SetText($"{(int)firstRoundDelay - i}s");
                yield return new WaitForSeconds(1);
            }
            _counterDisplay.SetPanelVisibility(false);
        }
        yield return new WaitForEndOfFrame();
        if (wave_i >= waveTotals.Count)
        {
            print($"FIRST WIN LOGIC");
            _win();
            yield break;
        }
        List<int> wave = waveTotals[wave_i];
        List<int> available = new List<int>();
        for (int i = 0; i < orderedPrefabs.Count; i++)
        {
            if (wave[i] != 0)
                available.Add(i);
        }

        OnRoundStarted.Invoke();
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
        //We have finished spawning this wave
        
        //Wait until it's finished to spawn another
        while (enemiesRemaining() != 0)
        {
            yield return new WaitForSeconds(2f);
        }
        OnRoundEnded.Invoke();
        wave_i++;
        if (wave_i >= waveTotals.Count)
        {
            print($"SECOND WIN LOGIC");
            _win();
            yield break;
        }
        _counterDisplay.SetPanelVisibility(true);
        for (int i = 0; i < waveDelay; i++)
        {
            _counterDisplay.SetText($"{waveDelay - i}s");
            yield return new WaitForSeconds(1);
        }
        _counterDisplay.SetPanelVisibility(false);
        
        StartCoroutine(WaveLoop());
    }
    
    
    /// <summary>
    /// How many enemies are left
    /// </summary>
    int enemiesRemaining()
    {
        int c = 0;
        foreach (var spawn in _spawnPoints)
            c += spawn.enemyParent.childCount;
        return c;
    }
    
}
