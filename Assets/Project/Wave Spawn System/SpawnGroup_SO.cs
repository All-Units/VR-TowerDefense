using UnityEngine;

/// <summary>
/// A Spawn group is the container for what spawnables will appear in that WaveGroup.
/// </summary>
public abstract class SpawnGroup_SO : ScriptableObject
{
    public DelayManifest delay;
    private SpawnDelay _delay;
    public SpawnPointData spawnPointData;
    
    public void StartGroup()
    {
        _delay = DelayFactory.Construct(delay);

        Debug.Log($"Starting delay for group: {name}! ");
        _delay.StartDelay();
    }

    public bool GroupReady() => _delay.IsComplete();
    public abstract GameObject[] GetSpawnables();
    public abstract EnemySpawnManifest GetSpawnManifest();
}

public struct EnemySpawnManifest
{
    public GameObject[] spawnables;
    public SpawnPointData spawnPointData;
}