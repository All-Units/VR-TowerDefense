using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    [SerializeField] private List<StatTracker> stats = new();
    

    private void Start()
    {
        stats.AddRange(StatTrackerHolder.BaseStats.trackers);
        IEnumerator _WaitToStart()
        {
            yield return null;
            yield return null;
            foreach (var stat in stats)
            {
                stat.ResetTotal();
                stat.Initialize(true);
            }
        }
        StartCoroutine(_WaitToStart());
        
        Deserialize();
    }

    private void OnDestroy()
    {
        foreach (var stat in stats)
        {
            stat.ClearTracker();
        }
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


