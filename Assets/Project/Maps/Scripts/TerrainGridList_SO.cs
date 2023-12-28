using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Levels/Terrain Grid List")]
public class TerrainGridList_SO :ScriptableObject
{
    public Material texture;
    public List<GameObject> terrainSquares;
}