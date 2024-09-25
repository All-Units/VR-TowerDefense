using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Tower Player Weapon Kills")]
public class TowerPlayerWeaponKillsTracker : StatTracker
{
    [SerializeField] TowerPlayerWeapon trackedTowerTakeoverObject;
    protected override void InitTracker()
    {
        TowerPlayerWeapon.onKill += OnKill;
    }

    public override void Print()
    {
        Debug.Log($"{statName}: {total}");
    }

    private void OnKill(TowerPlayerWeapon towerTakeoverObject, Enemy e)
    {
        if (trackedTowerTakeoverObject.Power != null && trackedTowerTakeoverObject.Power == towerTakeoverObject.Power)
        {
            Debug.Log($"OnKill: {trackedTowerTakeoverObject}");
            total++;
            InventoryManager.SetDebugText($"Set {trackedTowerTakeoverObject} total to: {total}");
            return;
        }
        else if(trackedTowerTakeoverObject.Data == towerTakeoverObject.Data)
        {
            Debug.Log($"OnKill: {trackedTowerTakeoverObject}");
            total++;
            InventoryManager.SetDebugText($"Set {trackedTowerTakeoverObject} total to: {total}");
        }
        else
        {
            InventoryManager.SetDebugText($"KILLED BY: {towerTakeoverObject}");
        }
    }

    public override void ClearTracker()
    {
        TowerPlayerWeapon.onKill -= OnKill;
    }
}