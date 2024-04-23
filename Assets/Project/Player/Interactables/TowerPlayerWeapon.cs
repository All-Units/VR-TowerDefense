using System;
using UnityEngine;

public class TowerPlayerWeapon : MonoBehaviour
{
    [SerializeField] private PlayerItem_SO data;
    public static Action<TowerPlayerWeapon, Enemy> onKill;

    public void OnPickUp()
    {
        // If data contains a ammo pouch, turn on ammo
        if (data.HasItemPouch())
            InventoryManager.instance.ActivateItemExtra(data);
    }

    public void OnDrop()
    {
        // If data contains an ammo pouch turn off
        if (data.HasItemPouch())
            InventoryManager.instance.DeactivateItemExtra(data);
    }

    public void OnKill(Enemy enemy)
    {
        onKill?.Invoke(this, enemy);
    }
}
