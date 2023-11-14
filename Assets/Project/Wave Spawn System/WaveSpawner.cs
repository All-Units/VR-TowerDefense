using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// This is the control center for the new waves system. It will read a levelSpawn data and iterate through the waves appropriately firing off events that can be used by other systems.
///
/// Todo:
/// 1) Parse the level data
/// 2) Spawn enemies
/// 3) Hook up start sequence
/// 4) Hook up the end sequence
/// </summary>
public class WaveSpawner : MonoBehaviour
{
    public LevelSpawn_SO levelData;

    private void Start()
    {
        levelData.StartWave();
        StartCoroutine(SpawnWave(levelData.GetNextWave()));
    }

    IEnumerator SpawnWave(WaveSpawn_SO waveSpawn)
    {
        waveSpawn.StartWave();

        while (waveSpawn.HasMoreWaveGroups())
        {
            var group = waveSpawn.GetNextWaveGroup();
            group.StartGroup();
            while (group.GroupReady() == false)
            {
                yield return null;
            }
            
            SpawnGroup(group.GetSpawnManifest());
        }

        if (levelData.HasMoreWave())
            StartCoroutine(SpawnWave(levelData.GetNextWave()));
    }

    private void SpawnGroup(EnemySpawnManifest manifest)
    {
        Debug.Log($"Spawning!");
    }
}

