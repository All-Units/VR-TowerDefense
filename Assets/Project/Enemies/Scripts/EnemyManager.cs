using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    #region InspectorFields
    [SerializeField] private LevelSpawn_SO levelData;
    [SerializeField] InputActionReference skipRoundButton;
    [SerializeField] float _timeToSkipRound = 0.7f;
    InputAction skipRoundAction => Utilities.GetInputAction(skipRoundButton);

    #endregion

    #region PublicStats
    public static EnemyManager instance;
    /// <summary>
    /// The current number of living enemies
    /// Null-safe
    /// </summary>
    public static int CurrentEnemyCount => (instance != null) ? instance.Enemies.Count : 0;
    
    /// <summary>
    /// Player-facing wave count, offset by 1
    /// </summary>
    public static int CurrentWave => instance ? instance._wave_i + 1 : -1;
    
    /// <summary>
    /// Returns the time until next wave
    /// </summary>
    public static int TimeUntilNextWave => (instance && instance._wave_i < instance.levelData.waveStructs.Count) 
    ? instance.levelData.waveStructs[instance._wave_i].preWaveDelay : -1;
    
    /// <summary>
    /// Gets the wave complete bonus for the last wave completed, or 0 if just started
    /// </summary>
    public static int LastWaveBonus => _LastWaveBonus();
    static int _LastWaveBonus()
    {
        if (instance == null) return 0;
        if (instance._wave_i == 0)
            return instance.levelData.waveStructs[instance._wave_i].WaveCompleteBounty;
        return instance.levelData.waveStructs[instance._wave_i - 1].WaveCompleteBounty;
    }
    /// <summary>
    /// The number of enemies currently alive
    /// </summary>
    public HashSet<Enemy> Enemies = new HashSet<Enemy>();
    public List<SpawnPointData> SpawnPoints = new List<SpawnPointData>();
    #endregion


    #region UnityEvents
    public static UnityEvent OnGameStart = new ();
    public static UnityEvent OnFirstRoundStarted = new ();
    public static UnityEvent OnRoundStarted = new ();
    public static UnityEvent OnRoundEnded = new ();

    public UnityEvent OnEnemySpawned = new ();
    public UnityEvent OnEnemyKilled = new ();

    public static void EnemySpawned(Enemy enemy)
    {
        if (instance)
        {
            instance._EnemySpawned(enemy);
        }
    }

    private void _EnemySpawned(Enemy enemy)
    {
        Enemies.Add(enemy);
        OnEnemySpawned.Invoke();
    }

    public static void EnemyKilled(Enemy enemy)
    {
        if(instance)
            instance._EnemyKilled(enemy);
    }

    private void _EnemyKilled(Enemy enemy)
    {
        if (Enemies.Contains(enemy))
            Enemies.Remove(enemy);
        
        OnEnemyKilled.Invoke();
    }
    
    private void Awake()
    {
        instance = this;
        SpawnPoints = new List<SpawnPointData>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(_LevelCoroutine());

        OnRoundEnded.AddListener(() => CurrencyManager.GiveToPlayer(levelData.waveStructs[_wave_i].WaveCompleteBounty));

        skipRoundAction.started += SkipRoundAction_started;
        skipRoundAction.canceled += SkipRoundAction_canceled;

    }

    

    private void Update()
    {
        IsCurrentRoundComplete = _currentWaveComplete;
        WaveroutineCount = _currentWaves.Count;
        EnemiesRemaining = Enemies.Count;

    }

    #endregion

    #region LevelCoroutines
    //Internal variables
    bool _currentWaveComplete = false;
    public bool IsCurrentRoundComplete;
    public int WaveroutineCount;
    public int EnemiesRemaining;
    public static int _public_wave_i => instance._wave_i;
    public static void SetWave_i(int i)
    {
        instance._wave_i = i;
    }
    public static bool IsWaveValid(int i)
    {
        if (instance == null) return false;
        return i < instance.levelData.waveStructs.Count;
    }
    int _wave_i = 0;
    public static bool SkipToNextRound = false;
    IEnumerator _LevelCoroutine()
    {
        //Give our starting gold
        CurrencyManager.GiveToPlayer(levelData.StartingGold);
        _currentWaveComplete = true;
        //Waiting a few frames in case _wave_i is overriden by a save file
        yield return new WaitForSeconds(0.1f);

        //Game started
        OnGameStart.Invoke();
        while (_wave_i < levelData.waveStructs.Count)
        {
            //Get the current wave
            var wave = levelData.waveStructs[_wave_i];

            //Wait n seconds to start wave
            float t = 0;
            while (t <= wave.preWaveDelay)
            {
                yield return null;
                if (XRPauseMenu.IsPaused == false)
                    t += Time.deltaTime;
                if (SkipToNextRound)
                {
                    SkipToNextRound = false;
                    break;
                }
            }
            //print($"Starting wave {CurrentWave}");
            //Start the wave logic
            StartCoroutine(_WaveCoroutine(wave));

            //Wait until the current wave is complete before starting another
            while (_currentWaveComplete == false)
                yield return null;
            
            //Next wave
            _wave_i++;
            //print($"Waited until last wave done, starting wave {CurrentWave}");

        }
        // print("YOU WIN!!!!!!");
        _win();
    }

    HashSet<IEnumerator> _currentWaves = new HashSet<IEnumerator>();
    /// <summary>
    /// Set whenever another wave is started, used to get self
    /// </summary>
    IEnumerator _lastSpawnedWave;
    LinkedList<IEnumerator> _subwaves = new LinkedList<IEnumerator>();
    IEnumerator _WaveCoroutine(WaveStruct wave)
    {
        //Wave started Events
        if (_wave_i == 0)
            OnFirstRoundStarted.Invoke();
        OnRoundStarted.Invoke();
        _currentWaveComplete = false;
        // print($"Started wave {_wave_i + 1}");
        List<GameObject> toSpawn = _GetSpawnList(wave.enemies);

        //Start sequentially
        bool first = true;
        //Start all subwaves
        foreach (SubWave subWave in wave.subWaves)
        {
            var coroutine = _SubwaveCoroutine(subWave);
            _subwaves.AddLast(coroutine);
            if (first)
            {
                StartCoroutine(coroutine);
                first = false;
                _subwaves.RemoveFirst();
            }
        }

        //Start main wave
        var sizes = levelData.GetGroupSizes(_wave_i);
        var rate = levelData.GetSpawnRate(_wave_i);
        _spawns = levelData.waveStructs[_wave_i].spawnPoints;
        var waveStruct = new _waveStruct();
        waveStruct.toSpawn = toSpawn;
        waveStruct.rate = rate;
        waveStruct.groupSizes = sizes;
        waveStruct.spawnPoints = wave.spawnPoints;
        var waveRoutine = _spawnWave(waveStruct);


        StartCoroutine(waveRoutine);
        _lastSpawnedWave = waveRoutine;
        float startTime = Time.time;
        //Wait until all waves have finished
        yield return new WaitForSeconds(1f);
        while (Enemies.Count > 0 || _currentWaves.Count > 0)
        {
            yield return null;
        }

        print($"Finished wave {CurrentWave} in {Time.time - startTime}s. Enemies left: {Enemies.Count}");
        _currentWaveComplete = true;
        OnRoundEnded.Invoke();
        yield return null;
        _spawns = null;
    }
    
    IEnumerator _SubwaveCoroutine(SubWave subWave)
    {
        //print($"Started subwave");
        var toSpawn = _GetSpawnList(subWave.enemies);
        _spawns = subWave.spawnPoints;
        var waveStuct = new _waveStruct();
        waveStuct.toSpawn = toSpawn;
        waveStuct.spawnPoints = subWave.spawnPoints;
        waveStuct.groupSizes = subWave.groupSizes;
        waveStuct.rate = subWave.spawnRate;
        //var wave = _spawnWave(toSpawn, subWave.groupSizes, subWave.spawnRate);
        var wave = _spawnWave(waveStuct);
        //_currentWaves.Add(wave);

        //Delay logic
        //print($"Started subwave, delay type {subWave.delayType.ToString()}");
        if (subWave.delayType == DelayType.TimeDelay)
            yield return new WaitForSeconds(subWave.DelayCount);
        else if (subWave.delayType == DelayType.EnemiesRemaining)
        {
            //We have to get to the threshold first, i.e. enough have spawned
            //print("Started waiting for enemies remaining");
            while (Enemies.Count <= subWave.DelayCount)
            {
                yield return null; 
            }
            yield return new WaitForSeconds(5f);
            //print("There are enough enemies alive, waiting until body count drops");
            //Wait in place until the enemy count drops to the desired
            while (Enemies.Count > subWave.DelayCount)
            {
                yield return null;
            }
            //print($"Waited until there were {subWave.DelayCount} enemies alive before spawning wave");
        }
        //Start timer for next subwave
        if (_subwaves.Count > 0)
        {
            var next = _subwaves.First();
            StartCoroutine(next);
            _subwaves.RemoveFirst();
            //print($"Started next subwave delay, there are {_subwaves.Count} remaining");
        }

        //print("Actually started subwave");
        //Start subwave


        _lastSpawnedWave = wave;
        StartCoroutine(wave);
        yield return null;
        _spawns = null;
    }
    List<SpawnPointData> _spawns = new List<SpawnPointData>();
    struct _waveStruct
    {

        public List<GameObject> toSpawn;
        public List<SpawnPointData> spawnPoints;
        public Vector2Int groupSizes;
        public float rate;

    }
    IEnumerator _spawnWave(_waveStruct wave)
    {
        //Init self logic

        var self = _lastSpawnedWave;
        if (_currentWaves.Contains(self) == false)
            _currentWaves.Add(self);
        int count = wave.toSpawn.Count;
        //print($"Pool of {count} in wave {CurrentWave}. Subwaves remaining {_subwaves.Count}");
        float time = Time.time;
        //While there are still enemies to spawn, keep spawning
        while (wave.toSpawn.Count > 0)
        {
            while (XRPauseMenu.IsPaused) yield return null;
            int groupSize = Random.Range(wave.groupSizes.x, wave.groupSizes.y + 1);
            int startSize = wave.toSpawn.Count;
            for (int i = 0; i < groupSize; i++)
            {
                //If we've run out of Gregs to spawn, break
                if (wave.toSpawn.Count == 0)
                    break;
                int index = wave.toSpawn.GetRandomIndex();
                SpawnEnemy(wave.toSpawn[index], wave.spawnPoints);
                wave.toSpawn.RemoveAt(index);
            }
            yield return new WaitForSeconds(wave.rate);
        }

        //Need to remove ourselves from list of active waves
        _currentWaves.Remove(self);
        // print($"Finished (sub)wave, removing self");
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
        {
            return SpawnPoints.GetRandom();
        }
        
        //Else choose from the specific list
        var points = spawnPoints;
        return points.GetRandom();
    }
    public void SpawnEnemy(GameObject enemyPrefab, List<SpawnPointData> spawns = null)
    {
        SpawnPointData point = _GetSpawnPoint(spawns);
        GameObject enemy = Instantiate(enemyPrefab, point.enemyParent);
        float r = enemy.GetComponent<CapsuleCollider>().radius;
        Vector3 pos = point.SpawnPoint.GetPoint(r);
        enemy.transform.position = pos + Vector3.up;
        var e = enemy.GetComponent<Enemy>();
        if (e)
            e._SetTarget(point.SpawnPoint.nextPoint);
        e.gameObject.name = $"{e.gameObject.name.Replace("(Clone)", "")} {e.transform.GetSiblingIndex()}";
    }
    void _win()
    {
        GameStateManager.WinGame();
    }

    #endregion
    #region SkipRoundLogic

    bool isSkipPressed = false;
    Transform cam => InventoryManager.instance.playerCameraTransform;


    GameObject _currentSkipPanel = null;
    public bool IS_TUTORIAL = false;
    public bool SKIP_TUTORIAL_IS_COMPLETE = false;
    private void SkipRoundAction_started(InputAction.CallbackContext obj)
    {
        if (_currentWaveComplete == false) return;

        //If we are the tutorial level but we haven't unlocked skipping level, don't do it accidentally
        if (IS_TUTORIAL && SKIP_TUTORIAL_IS_COMPLETE == false) return;
        XRPauseMenu.ForceResume();
        isSkipPressed = true;
        _currentSkipRoutine = _SkipRound();
        StartCoroutine(_currentSkipRoutine);
        
    }
    private void SkipRoundAction_canceled(InputAction.CallbackContext obj)
    {
        isSkipPressed = false;
        
    }
    IEnumerator _currentSkipRoutine = null;
    IEnumerator _SkipRound()
    {
        GameObject panel = Instantiate(skipPanelPrefab);
        _currentSkipPanel = panel;
        panel.transform.position = cam.position;

        Vector3 euler = new Vector3(0f, cam.eulerAngles.y, 0f);
        panel.transform.eulerAngles = euler;
        
        float t = 0f;
        Image fill = panel.GetComponentInChildren<Image>();
        fill.fillAmount = 0f;
        while (t <= _timeToSkipRound)
        {
            if (isSkipPressed == false)
                break;
            yield return null;
            t += Time.deltaTime;
            fill.fillAmount = math.lerp(0f, 1f, (t / _timeToSkipRound));
        }
        panel.DestroyAfter(0.1f);
        if (t >= _timeToSkipRound)
        {
            XRPauseMenu.ForceResume();
            yield return new WaitForSeconds(0.15f);
            SkipToNextRound = true; 
        }
        
        _currentSkipRoutine = null;
    }
    GameObject skipPanelPrefab => Resources.Load<GameObject>("Prefabs/SkipRoundCanvas");


    #endregion

    #region Legacy
    [HideInInspector]
    public bool IsStressTest = false;
    private float firstRoundDelay = 5f;


    [HideInInspector]
    public float enemySpawnDelay = 1f;
    [Tooltip("The number of seconds between rounds")]
    [HideInInspector]
    private int waveDelay = 20;

    public static int WaveDelay => instance ? instance.waveDelay : -1;
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

    public static int MaxWaves => instance ? instance.waveTotals.Count : -1;
    private int old_wave_i = 0;


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
            // print($"FIRST WIN LOGIC");
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
            while (XRPauseMenu.IsPaused) yield return null;
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
            // print($"SECOND WIN LOGIC");
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

    public static void HideEnemies()
    {
        if (instance == null) return;

        instance._HideEnemies();
    }

    private void _HideEnemies()
    {
        foreach (var enemy in Enemies)
        {
            enemy.Hide();
        }
    }
}