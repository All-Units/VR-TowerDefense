using System.Collections;
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
            total++;
            //Debug.Log($"A MATCHING POWER: {trackedTowerTakeoverObject.Power} to {towerTakeoverObject.gameObject.name}", towerTakeoverObject);
            return;
        }
        else if(trackedTowerTakeoverObject.Data != null && trackedTowerTakeoverObject.Data == towerTakeoverObject.Data)
        {
            total++;
            return;
        }

        return;
        InventoryManager.SetDebugText($"KILLED BY: {towerTakeoverObject}.\n" +
            $"THIS IS AN ERROR!");
        Debug.LogError($"No data or power on {towerTakeoverObject.transform.FullPath()}. We are {trackedTowerTakeoverObject.name}. \t" +
            $"TrackedTowerPower == null? {trackedTowerTakeoverObject.Power == null}. " +
            $"was TowerData null? {trackedTowerTakeoverObject.Data == null} ", towerTakeoverObject);
        
    }

    public override void ClearTracker()
    {
        TowerPlayerWeapon.onKill -= OnKill;
    }
}