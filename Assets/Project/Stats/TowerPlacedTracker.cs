using Project.Towers.Scripts;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Tower Placed")]
public class TowerPlacedTracker : TowerTracker
{
    protected override void InitTracker()
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

    public override void ClearTracker()
    {
        TowerSpawnManager.OnTowerSpawned -= OnTowerSpawned;
    }
}