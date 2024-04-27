using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    [SerializeField] private List<StatTracker> stats = new();
    

    private void Start()
    {
        stats.AddRange(StatTrackerHolder.BaseStats.trackers);
        foreach (var stat in stats)
        {
            stat.ResetTotal();
            stat.Initialize(true);
        }
        Deserialize();
    }

    private void OnDestroy()
    {
        Serialize();
    }

    private void Serialize()
    {
        foreach (var stat in stats)
        {
            stat.Serialize();
        }
    }

    private void Deserialize()
    {
        foreach (var stat in stats)
        { 
            stat.Deserialize();
        }
    }
}


