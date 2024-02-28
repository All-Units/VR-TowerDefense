using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Enemy Killed")]
public class KillsTracker : StatTracker
{
    [SerializeField] private EnemyDTO _enemyToTrack;
    
    protected override void InitTracker()
    {
        Enemy.OnDeath += OnDeath;
        //Debug.Log($"Initted tracker for {_enemyToTrack}");
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
            //Debug.Log($"{_enemyToTrack} killed, bringing total to {total}");
        }
        

    }
}