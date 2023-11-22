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
    public int GetBounty(int i)
    {
        return waveStructs[i].WaveCompleteBounty;
    }
    public void EditBounty(int i, int bounty) {
        var wave = waveStructs[i];
        wave.WaveCompleteBounty = bounty;
        waveStructs[i] = wave;
    }

    public WaveSpawn_SO GetNextWave() => _queue.Dequeue();
    public bool HasMoreWave() => _queue.Count > 0;
}

[Serializable]
public struct WaveStruct
{
    public List<EnemyQuant> enemies;
    public List<SubWave> subWaves;
    public int WaveCompleteBounty;
    public List<SpawnPointData> spawnPoints;
}
[Serializable]
public struct SubWave
{
    public List<EnemyQuant> enemies;
    public DelayType delayType;
    public int DelayCount;
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