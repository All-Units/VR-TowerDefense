using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Spawn System/Level")]
public class LevelSpawn_SO : ScriptableObject
{
    [SerializeField] private List<WaveSpawn_SO> waves;

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
        foreach (var wave in waves)
        {
            _queue.Enqueue(wave);
        }
        
        Debug.Log($"Starting Level {name}! Waves: {_queue.Count}");
    }

    public WaveSpawn_SO GetNextWave() => _queue.Dequeue();
    public bool HasMoreWave() => _queue.Count > 0;
}