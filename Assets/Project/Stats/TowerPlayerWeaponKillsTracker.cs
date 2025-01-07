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
            return;
        }
        else if(trackedTowerTakeoverObject.Data != null && trackedTowerTakeoverObject.Data == towerTakeoverObject.Data)
        {
            total++;
            return;
        }

        return;
        
        
    }

    public override void ClearTracker()
    {
        TowerPlayerWeapon.onKill -= OnKill;
    }
}