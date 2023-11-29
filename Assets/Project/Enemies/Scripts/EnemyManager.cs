using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour
{
    #region InspectorFields
    [SerializeField] private LevelSpawn_SO levelData;

    public int CurrentEnemiesAlive = 0;
    void onGregChange() { CurrentEnemiesAlive = EnemyCount; }
    
    #endregion

    

    


    #region PublicStats
    public static EnemyManager instance;
    /// <summary>
    /// Player-facing wave count, offset by 1
    /// </summary>
    public static int CurrentWave => instance._wave_i + 1;
    /// <summary>
    /// The number of enemies currently alive
    /// </summary>
    public static int EnemyCount = 0;

    public static List<SpawnPointData> SpawnPoints = new List<SpawnPointData>();
    #endregion


    #region UnityEvents
    public static UnityEvent OnGameStart = new UnityEvent();
    public static UnityEvent OnFirstRoundStarted = new UnityEvent();
    public static UnityEvent OnRoundStarted = new UnityEvent();
    public static UnityEvent OnRoundEnded = new UnityEvent();

    public static UnityEvent OnGregSpawned = new UnityEvent();
    public static void GregSpawned() {  OnGregSpawned.Invoke(); }
    public static UnityEvent OnGregKilled = new UnityEvent();
    public static void GregKilled() {  OnGregKilled.Invoke(); }


    private void Awake()
    {
        instance = this;
        SpawnPoints = new List<SpawnPointData>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(_LevelCoroutine());
        OnGregSpawned.AddListener(onGregChange);
        OnGregKilled.AddListener(onGregChange);
        //parseCSV();
        //_spawnPoints = GetComponentsInChildren<SpawnPoint>().ToList();
        
        
        //StartCoroutine(WaveLoop());
        
    }

    #endregion



   

    #region LevelCoroutines
    //Internal variables
    bool _currentWaveComplete = false;
    int _wave_i = 0;
    IEnumerator _LevelCoroutine()
    {
        //Game started
        OnGameStart.Invoke();
        while (_wave_i < levelData.waveStructs.Count)
        {
            //Get the current wave
            var wave = levelData.waveStructs[_wave_i];

            //Wait n seconds to start wave
            yield return new WaitForSeconds(wave.preWaveDelay);
            //Start the wave logic
            StartCoroutine(_WaveCoroutine(wave));

            //Wait until the current wave is complete before starting another
            while (_currentWaveComplete == false)
                yield return null;

            //Next wave
            _wave_i++;
        }
        print("YOU WIN!!!!!!");
    }

    HashSet<IEnumerator> _currentWaves = new HashSet<IEnumerator>();
    /// <summary>
    /// Set whenever another wave is started, used to get self
    /// </summary>
    IEnumerator _lastSpawnedWave;
    IEnumerator _WaveCoroutine(WaveStruct wave)
    {
        //Wave started Events
        if (_wave_i == 0)
            OnFirstRoundStarted.Invoke();
        OnRoundStarted.Invoke();
        _currentWaveComplete = false;
        print($"Started wave {_wave_i + 1}");
        List<GameObject> toSpawn = _GetSpawnList(wave.enemies);

        //Start all subwaves
        foreach (SubWave subWave in wave.subWaves)
        {
            StartCoroutine(_SubwaveCoroutine(subWave));
        }

        //Start main wave
        var sizes = levelData.GetGroupSizes(_wave_i);
        var rate = levelData.GetSpawnRate(_wave_i);
        _spawns = levelData.waveStructs[_wave_i].spawnPoints;
        var waveRoutine = _spawnWave(toSpawn, sizes, rate);

        
        StartCoroutine(waveRoutine);
        _lastSpawnedWave = waveRoutine;
        float startTime = Time.time;
        //Wait until all waves have finished
        while (_currentWaves.Count > 0)
        {
            yield return null;
        }
        
        print($"Finished wave {CurrentWave} in {Time.time - startTime}s");
        _currentWaveComplete = true;
        OnRoundEnded.Invoke();
        yield return null;
        _spawns = null;
    }
    IEnumerator _SubwaveCoroutine(SubWave subWave)
    {
         var toSpawn = _GetSpawnList(subWave.enemies);
        _spawns = subWave.spawnPoints;
        var wave = _spawnWave(toSpawn, subWave.groupSizes, subWave.spawnRate);
        _currentWaves.Add(wave);
        if (subWave.delayType == DelayType.TimeDelay)
            yield return new WaitForSeconds(subWave.DelayCount);
        else
        {
            //We have to get to the threshold first, i.e. enough have spawned
            while (EnemyCount <= subWave.DelayCount)
                yield return null;
            print("Reached threshold, there are enough gregs alive");
            //Wait in place until the enemy count drops to the desired
            while (EnemyCount > subWave.DelayCount)
            {
                yield return null;
            }
            print($"Waited until there were {subWave.DelayCount} Gregs alive before spawning wave");
        }
       
        _lastSpawnedWave = wave;
        StartCoroutine(wave);
        yield return null;
        _spawns = null;
    }
    List<SpawnPointData> _spawns = new List<SpawnPointData>();
    IEnumerator _spawnWave(List<GameObject> toSpawn, Vector2Int groupSizes, float rate)
    {
        //Init self logic
        
        var self = _lastSpawnedWave;
        if (_currentWaves.Contains(self) == false)
            _currentWaves.Add(self);
        int count = toSpawn.Count;
        print($"Pool of {count} in wave {CurrentWave}");
        float time = Time.time;
        //While there are still enemies to spawn, keep spawning
        while (toSpawn.Count > 0)
        {
            int groupSize = Random.Range(groupSizes.x, groupSizes.y + 1);
            int startSize = toSpawn.Count;
            for (int i = 0; i < groupSize; i++)
            {
                //If we've run out of Gregs to spawn, break
                if (toSpawn.Count == 0)
                    break;
                int index = toSpawn.GetRandomIndex();
                SpawnEnemy(toSpawn[index], _spawns);
                toSpawn.RemoveAt(index);
            }
            yield return new WaitForSeconds(rate);
        }

        //Need to remove ourselves from list of active waves
        _currentWaves.Remove(self);

    }
    #endregion


    #region HelperFunctions
    GameObject _GetPrefab(EnemyType type)
    {
        return levelData.GetEnemyPrefab(type);
    }


    /// <summary>
    /// Populates a list of enemy prefabs to spawn
    /// </summary>
    /// <param name="enemies"></param>
    /// <returns></returns>
    List<GameObject> _GetSpawnList(List<EnemyQuant> enemies)
    {
        var toSpawn = new List<GameObject>();
        foreach (var enemy in enemies)
        {
            int count = Random.Range(enemy.amountToSpawn.x, enemy.amountToSpawn.y + 1);
            if (count == 0)
                continue;
            GameObject prefab = _GetPrefab(enemy.enemyType);
            for (int i = 0; i < count; i++)
            {
                toSpawn.Add(prefab);
            }
        }

        return toSpawn;
    }
    /// <summary>
    /// Gets a random valid spawn point for the current wave
    /// </summary>
    /// <param name="spawnPoints">Can override the list of points for sub-waves</param>
    /// <returns></returns>
    SpawnPointData _GetSpawnPoint(List<SpawnPointData> spawnPoints = null)
    {
       
        //If the list is empty, choose randomly from all
        if (spawnPoints == null || spawnPoints.Count == 0)
            return SpawnPoints.GetRandom();
        //Else choose from the specific list
        var points = spawnPoints;
        return points.GetRandom();
    }
    public void SpawnEnemy(GameObject enemyPrefab, List<SpawnPointData> spawns = null)
    {
        SpawnPointData point = _GetSpawnPoint(spawns);
        GameObject enemy = Instantiate(enemyPrefab, point.enemyParent);
        enemy.transform.position = point.PositionOffset(4);
        var e = enemy.GetComponent<BasicEnemy>();
        Minimap.SpawnHead(e);
        //SpawnHead(e);
        e.nextWaypoint = point.SpawnPoint.nextPoint;
    }

    #endregion

    #region Legacy
    [HideInInspector]
    public bool IsStressTest = false;
    private float firstRoundDelay = 5f;
    [HideInInspector]
    public int old_EnemyCount = 0;

    [HideInInspector]
    public float enemySpawnDelay = 1f;
    [Tooltip("The number of seconds between rounds")]
    [HideInInspector]
    private int waveDelay = 20;

    public static int WaveDelay => instance.waveDelay;
    [HideInInspector]
    private List<GameObject> enemyPrefabs;
    [HideInInspector]
    private GameObject nextRoundCounterPanel;
    [HideInInspector]
    private TextAsset levelCSV;
    [HideInInspector]
    private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
    //private WaveCounterDisplay _counterDisplay;

    public Dictionary<BasicEnemy, Transform> enemies = new Dictionary<BasicEnemy, Transform>();
    [HideInInspector]
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
            if (e)
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

    public static int MaxWaves => instance.waveTotals.Count;
    private int old_wave_i = 0;

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
            //if (_counterDisplay && _counterDisplay.enabled)
            //    _counterDisplay.SetPanelVisibility(true);
            for (int i = 0; i < (int)firstRoundDelay; i++)
            {
                //if (_counterDisplay && _counterDisplay.enabled)
                //    _counterDisplay.SetText($"{(int)firstRoundDelay - i}s");
                yield return new WaitForSeconds(1);
            }
            //if (_counterDisplay && _counterDisplay.enabled)
            //    _counterDisplay.SetPanelVisibility(false);
        }
        yield return new WaitForEndOfFrame();
        if (old_wave_i >= waveTotals.Count)
        {
            print($"FIRST WIN LOGIC");
            _win();
            yield break;
        }
        List<int> wave = waveTotals[old_wave_i];
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

        old_wave_i++;
        if (old_wave_i >= waveTotals.Count)
        {
            print($"SECOND WIN LOGIC");
            _win();
            yield break;
        }
        OnRoundEnded.Invoke();
        //if (_counterDisplay && _counterDisplay.enabled)
        //    _counterDisplay.SetPanelVisibility(true);
        for (int i = 0; i < waveDelay; i++)
        {
            //if (_counterDisplay && _counterDisplay.enabled)
            //    _counterDisplay.SetText($"{waveDelay - i}s");
            yield return new WaitForSeconds(1);
        }
        //if (_counterDisplay && _counterDisplay.enabled)
        //    _counterDisplay.SetPanelVisibility(false);

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

    public static void SpawnRandom()
    {
        instance.SpawnEnemy(instance.enemyPrefabs.GetRandom());
    }

    #endregion
}
