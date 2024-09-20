using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Tower Player Weapon Kills")]
public class TowerPlayerWeaponKillsTracker : StatTracker
{
    [SerializeField] TowerTakeoverObject trackedTowerTakeoverObject;
    protected override void InitTracker()
    {
        TowerTakeoverObject.OnKillWithItem += OnKill;
    }

    public override void Print()
    {
        Debug.Log($"{statName}: {total}");
    }

    private void OnKill(TowerTakeoverObject towerTakeoverObject)
    {
        if(trackedTowerTakeoverObject == towerTakeoverObject)
        {
            Debug.Log($"OnKill: {trackedTowerTakeoverObject}");
            total++;
        }
    }

    public override void ClearTracker()
    {
        TowerTakeoverObject.OnKillWithItem -= OnKill;
    }
}