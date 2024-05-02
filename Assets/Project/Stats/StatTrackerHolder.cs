using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Tracker Holder")]
public class StatTrackerHolder : ScriptableObject
{
    public List<StatTracker> trackers = new();

#if UNITY_EDITOR
    public static StatTrackerHolder BaseStats => AssetDatabase.LoadAssetAtPath<StatTrackerHolder>("Assets/Project/Stats/SO/Trackers/Base Stats.asset");
#else
    public static StatTrackerHolder BaseStats => Resources.Load<StatTrackerHolder>("Base Stats.asset");
#endif
}