using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Display")]
public class StatDisplayModel : ScriptableObject
{
    public string displayName;
    public List<StatTracker> statTrackers = new();
}