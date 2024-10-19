using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Tower Destroyed")]
public class TowerDestroyedTracker : TowerTracker
{
    public int DestroyedAsPlayerCount = 0;
    public string LostAsPlayerSuffix = "(as player)";
    public override string GetDisplayString()
    {
        string s = base.GetDisplayString();
        if (DestroyedAsPlayerCount <= 0)
            return s;
        return s + $"\t{LostAsPlayerSuffix}: {DestroyedAsPlayerCount.PrettyNumber()}";
    }
    public override void ResetTotal()
    {
        total = 0;
        DestroyedAsPlayerCount = 0;
        SerializeIfChanged();
    }
    const string _playerSuffix = "_Player";
    public override void Serialize()
    {
        //If we aren't player controller, don't track that and override anything that did (some legacy saves)
        if (_towerToTrack.towerPrefab != null && (_towerToTrack.towerPrefab is PlayerControllableTower) == false)
        {
            DestroyedAsPlayerCount = 0;
        }
        PlayerPrefs.SetInt($"{key}{_playerSuffix}", DestroyedAsPlayerCount);
        base.Serialize();
    }
    public override void Deserialize()
    {
        DestroyedAsPlayerCount = PlayerPrefs.GetInt($"{key}{_playerSuffix}", 0);
        //If we aren't player controller, don't track that and override anything that did (some legacy saves)
        if (_towerToTrack.towerPrefab != null && (_towerToTrack.towerPrefab is PlayerControllableTower) == false)
        {
            DestroyedAsPlayerCount = 0;
            PlayerPrefs.SetInt($"{key}{_playerSuffix}", 0);
        }
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
            if ((_towerToTrack.towerPrefab is PlayerControllableTower) == false) {
                //Debug.Log($"{_towerToTrack.towerPrefab} is not a PCT, doing nothing");
                return; }
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