using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Levels/Placeable Prefab List")]
public class PrefabList_SO : ScriptableObject
{
    public List<GameObject> prefabs;

    public GameObject GetRandom()
    {
        return prefabs.GetRandom();
    }
}