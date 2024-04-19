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

    private void OnDeath(Enemy obj)
    {
        if (obj.Stats == _enemyToTrack)
        {
            total++;
        }
    }
}