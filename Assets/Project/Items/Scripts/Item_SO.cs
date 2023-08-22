using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "SO/Item", fileName = "New Item")]
public class Item_SO : ScriptableObject
{
    public BaseItem itemPrefab;
    public GameObject itemIconPrefab;
    public bool twoHanded = false;
}
