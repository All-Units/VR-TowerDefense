using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    public TilesSO tile_list;

    [SerializeField] private bool canBePlacedOn = true;
    public Tower currentTower = null;

    public bool selectable => canBePlacedOn && currentTower == null;
}
