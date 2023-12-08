using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Spawn System/Level")]
public class LevelSpawn_SO : ScriptableObject
{
    [SerializeField]
    public List<EnemyPrefabByType> enemyPrefabs = new List<EnemyPrefabByType>();
    public List<WaveStruct> waveStructs = new List<WaveStruct>();  
    [ExecuteInEditMode, ContextMenu("Test")]
    public void TestRunSpawn()
    {
        StartWave();

        while (HasMoreWave())
        {
            var nextWave = GetNextWave();
            
            nextWave.StartWave();

            while (nextWave.HasMoreWaveGroups())
            {
                var nextGroup = nextWave.GetNextWaveGroup();
                nextGroup.StartGroup();
                
                Debug.Log($"Spawning: {nextGroup.GetSpawnables().ToList().Count} enemies at Spawnpoint: {(nextGroup.spawnPointData ? nextGroup.spawnPointData.name : "Null")}");
            }
        }
    }
    
    private Queue<WaveSpawn_SO> _queue = new Queue<WaveSpawn_SO>();

    public void StartWave()
    {
        _queue.Clear();
        //Just to get it to compile
        foreach (var wave in new List<WaveSpawn_SO>())
        {
            _queue.Enqueue(wave);
        }
        
        Debug.Log($"Starting Level {name}! Waves: {_queue.Count}");
    }
    #region GettersAndSetters
    public int GetBounty(int i)
    {
        return waveStructs[i].WaveCompleteBounty;
    }
    public void EditBounty(int i, int bounty) {
        var wave = waveStructs[i];
        wave.WaveCompleteBounty = bounty;
        waveStructs[i] = wave;
    }
    public int GetDelay(int i)
    {
        return waveStructs[i].preWaveDelay;
    }
    public void EditDelay(int i, int delay)
    {
        var wave = waveStructs[i];
        wave.preWaveDelay = delay;
        waveStructs[i] = wave;
    }
    public Vector2Int GetGroupSizes(int i)
    {
        return waveStructs[i].groupSizes;
    }
    
    public void EditGroupSizes(int i, Vector2Int sizes)
    {
        var wave = waveStructs[i];
        wave.groupSizes = sizes;
        waveStructs[i] = wave;
    }
    /// <summary>
    /// The time, in seconds, between spawning groups of Gregs
    /// </summary>
    /// <param name="i">The wave to get the spawn rate of</param>
    /// <returns></returns>
    public float GetSpawnRate(int i)
    {
        return waveStructs[i].spawnRate;
    }
    /// <summary>
    /// Sets the time, in seconds, between spawning groups of Gregs
    /// </summary>
    /// <param name="i"></param>
    /// <param name="spawnRate">the new time to set</param>
    public void EditSpawnRate(int i, float spawnRate)
    {
        var wave = waveStructs[i];
        wave.spawnRate = spawnRate;
        waveStructs[i] = wave;
    }
    /// <summary>
    /// Gets the prefab reference for a given enemy type
    /// </summary>
    /// <param name="type">The type of enemy to find the prefab of</param>
    /// <returns></returns>
    public GameObject GetEnemyPrefab(EnemyType type)
    {
        foreach (var e in enemyPrefabs)
        {
            if (e.type == type)
                return e.prefab;
        }
        return null;
    }
    #endregion

    public WaveSpawn_SO GetNextWave() => _queue.Dequeue();
    public bool HasMoreWave() => _queue.Count > 0;
}

[Serializable]
public struct WaveStruct
{
    public List<EnemyQuant> enemies;
    public List<SubWave> subWaves;
    public int WaveCompleteBounty;
    public int preWaveDelay;
    public Vector2Int groupSizes;
    /// <summary>
    /// The time, in seconds, between spawning groups of Gregs
    /// </summary>
    public float spawnRate;
    public List<SpawnPointData> spawnPoints;
}
[Serializable]
public struct SubWave
{
    public List<EnemyQuant> enemies;
    public DelayType delayType;
    public int DelayCount;
    public Vector2Int groupSizes;
    /// <summary>
    /// The time, in seconds, between spawning groups of Gregs
    /// </summary>
    public float spawnRate;
    public List<SpawnPointData> spawnPoints;
}

[Serializable]
public struct EnemyQuant
{
    public EnemyType enemyType;
    public Vector2Int amountToSpawn;
}

[Serializable] public enum EnemyType
{
    Greg,
    Sweg,
    Breg,
}

[Serializable]
public struct EnemyPrefabByType
{
    public GameObject prefab;
    public EnemyType type;
}

/// <summary>
/// The types of delays between subwaves
/// </summary>
[Serializable] public enum DelayType
{
    /// <summary>
    /// Waits a given number of seconds before spawning the sub-wave
    /// </summary>
    TimeDelay,
    /// <summary>
    /// Waits until a given number of Gregs are still alive before spawning the next sub-wave
    /// </summary>
    EnemiesRemaining
}