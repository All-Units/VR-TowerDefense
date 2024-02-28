using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    [SerializeField] private List<StatTracker> stats = new();

    private void Start()
    {
        foreach (var stat in stats)
        {
            stat.ResetTotal();
            stat.Initialize();
        }
        Deserialize();
        
        EnemyManager.OnRoundEnded.AddListener(OnRoundEnded);
    }

    private void OnDestroy()
    {
        Serialize();
        EnemyManager.OnRoundEnded.RemoveListener(OnRoundEnded);
    }

    private void OnRoundEnded()
    {
        /*foreach (var stat in stats)
        {
            stat.Print();
        }*/
    }

    private void Serialize()
    {
        foreach (var stat in stats)
        {
            PlayerPrefs.SetInt(stat.key, stat.total);
        }
    }

    private void Deserialize()
    {
        foreach (var stat in stats)
        { 
            stat.total = PlayerPrefs.GetInt(stat.key);
        }
    }
}


