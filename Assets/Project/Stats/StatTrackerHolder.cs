using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(StatTrackerHolder))]

public class StatTrackerHolderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (target is StatTrackerHolder stat)
        {
            bool _containsDuplicates = stat.trackers.Count != stat.trackers.ToHashSet().Count;
        
            if (_containsDuplicates && GUILayout.Button($"Purge duplicates?"))
            {
                List<StatTracker> _deepCopy = new List<StatTracker>();
                HashSet<StatTracker> _containsSet = new HashSet<StatTracker>();
                foreach (StatTracker tracker in stat.trackers)
                {
                    if (_containsSet.Contains(tracker)) continue;
                    _containsSet.Add(tracker);
                    _deepCopy.Add(tracker);
                }

                stat.trackers = _deepCopy;
                _containsSet = null;
            }
        
            
        }
        
    }
}
#endif

[CreateAssetMenu(menuName = "SO/Stats/Tracker Holder")]
public class StatTrackerHolder : ScriptableObject
{
    public List<StatTracker> trackers = new();
    
    public static StatTrackerHolder BaseStats => Resources.Load<StatTrackerHolder>("Base Stats");
}