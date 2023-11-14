using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawn group that returns the exact spawnable list in the exact order designed
/// </summary>
[CreateAssetMenu(menuName = "SO/Spawn System/Spawn Group/Direct")]
public class DirectSpawnGroup_SO : SpawnGroup_SO
{
    [SerializeField] private List<GameObject> spawnables;

    public override GameObject[] GetSpawnables()
    {
        return spawnables.ToArray();
    }

    public override EnemySpawnManifest GetSpawnManifest()
    {
        return new EnemySpawnManifest() { spawnables = GetSpawnables(), spawnPointData = spawnPointData };
    }
}