using Project.Towers.Scripts;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Tower Placed")]
public class TowerPlacedTracker : TowerTracker
{
    public override void Initialize()
    {
        TowerSpawnManager.OnTowerSpawned += OnTowerSpawned;
    }
    
    public override void Print()
    {
        Debug.Log($"{_towerToTrack.name} placed: {total}");
    }

    private void OnTowerSpawned(Tower_SO obj)
    {
        if (obj == _towerToTrack)
            total++;
    }
}