using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Spawn System/Wave")]
public class WaveSpawn_SO : ScriptableObject
{
    public List<SpawnGroup_SO> spawnGroups = new List<SpawnGroup_SO>();
    [Tooltip("Amount of money the player recieves for beating the wave")]
    public int WaveCompleteBounty = 50;
    private Queue<SpawnGroup_SO> _queue = new Queue<SpawnGroup_SO>();

    public void StartWave()
    {
        _queue.Clear();
        foreach (var spawnGroup in spawnGroups)
        {
            _queue.Enqueue(spawnGroup);
        }
        
        Debug.Log($"Starting Wave: {name}! Groups: {_queue}");
    }

    public SpawnGroup_SO GetNextWaveGroup() => _queue.Dequeue();
    public bool HasMoreWaveGroups() => _queue.Count > 0;
}