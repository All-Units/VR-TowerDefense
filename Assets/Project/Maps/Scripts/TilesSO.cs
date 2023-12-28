using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "tile_list", 
    menuName = "MapTile/tile_list", 
    order = 0)]
public class TilesSO : ScriptableObject
{
    public string listName;
    public List<GameObject> tiles = new List<GameObject>();
}
