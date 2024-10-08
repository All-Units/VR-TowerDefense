using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Tower Destroyed")]
public class TowerDestroyedTracker : TowerTracker
{
    public int DestroyedAsPlayerCount = 0;
    public string LostAsPlayerSuffix = "(as player)";
    public override void ResetTotal()
    {
        total = 0;
        DestroyedAsPlayerCount = 0;
        SerializeIfChanged();
    }
    const string _playerSuffix = "_Player";
    public override void Serialize()
    {
        PlayerPrefs.SetInt($"{key}{_playerSuffix}", total);
        base.Serialize();
    }
    public override void Deserialize()
    {
        DestroyedAsPlayerCount = PlayerPrefs.GetInt($"{key}{_playerSuffix}", 0);
        base.Deserialize();
    }
    protected override void InitTracker()
    {
        Tower.OnTowerDestroy += TowerOnTowerDestroy;
    }
    
    public override void Print()
    {
        Debug.Log($"{_towerToTrack.name} destroyed: {total}");
    }

    private void TowerOnTowerDestroy(Tower obj)
    {
        if (obj.dto == _towerToTrack && obj.healthController.CurrentHealth <= 0)
        {
            total++;
            if (obj is PlayerControllableTower pct && pct.isPlayerControlled)
            {
                DestroyedAsPlayerCount++;
            }
        }
            
    }

    public override void ClearTracker()
    {
        Tower.OnTowerDestroy -= TowerOnTowerDestroy;
    }
}