using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Tower Destroyed")]
public class TowerDestroyedTracker : TowerTracker
{
    public override void Initialize()
    {
        Tower.OnTowerDestroy += TowerOnTowerDestroy;
    }
    
    public override void Print()
    {
        Debug.Log($"{_towerToTrack.name} destroyed: {total}");
    }

    private void TowerOnTowerDestroy(Tower obj)
    {
        if (obj.dto == _towerToTrack)
            total++;
    }
}