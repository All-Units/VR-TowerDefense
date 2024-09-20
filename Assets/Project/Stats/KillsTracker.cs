using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Enemy Killed")]
public class KillsTracker : StatTracker
{
    [SerializeField] private EnemyDTO _enemyToTrack;
    
    protected override void InitTracker()
    {
        Enemy.OnDeath += OnDeath;
    }

    public override void Print()
    {
        Debug.Log($"{_enemyToTrack.name} kills: {total}");
    }
    static HashSet<Enemy> killed = new HashSet<Enemy>();
    private void OnDeath(Enemy obj)
    {
        //if (killed.Contains(obj)) { Debug.Log($"ALREADY CONTAINED {obj.gameObject.name}, returning", obj); return; }
        //else
        {
            //Debug.Log($"DID NOT contain {obj.gameObject.name}", obj);
        }
        if (obj.Stats == _enemyToTrack && killed.Contains(obj) == false)
        {
            killed.Add(obj);
            total++;
            
        }
        

    }

    public override void ClearTracker()
    {
        Enemy.OnDeath -= OnDeath;
        killed.Clear();
    }
}