using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Tracker Holder")]
public class StatTrackerHolder : ScriptableObject
{
    public List<StatTracker> trackers = new();

    public static StatTrackerHolder BaseStats => Resources.Load<StatTrackerHolder>("Stats/Base Stats.asset");
}