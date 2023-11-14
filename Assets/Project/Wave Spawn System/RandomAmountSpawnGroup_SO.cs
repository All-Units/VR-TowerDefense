using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawn group that returns the exact spawnable list in the exact order designed
/// </summary>
[CreateAssetMenu(menuName = "SO/Spawn System/Spawn Group/Random Amount")]
public class RandomAmountSpawnGroup_SO : SpawnGroup_SO
{
    [SerializeField] private List<GameObject> spawnables;
    [SerializeField] private int min = 0, max = 10;

    public override GameObject[] GetSpawnables()
    {
        var amount = Random.Range(min, max);
        var ret = new List<GameObject>();
        
        for (var i = 0; i <= amount; i++)
        {
            ret.Add(spawnables.GetRandom());    
        }
        return ret.ToArray();
    }

    public override EnemySpawnManifest GetSpawnManifest()
    {
        return new EnemySpawnManifest() { spawnables = GetSpawnables(), spawnPointData = spawnPointData };
    }
}