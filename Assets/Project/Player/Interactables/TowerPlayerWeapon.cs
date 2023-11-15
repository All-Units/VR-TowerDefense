using UnityEngine;

public class TowerPlayerWeapon : MonoBehaviour
{
    [SerializeField] private PlayerItem_SO data;

    public void OnPickUp()
    {
        // If data contains a ammo pouch, turn on ammo
        if (data.HasItemPouch())
            InventoryManager.instance.ActivateItemExtra(data.ammoPouch);
    }

    public void OnDrop()
    {
        // If data contains an ammo pouch turn off
        if (data.HasItemPouch())
            InventoryManager.instance.DeactivateItemExtra(data.ammoPouch);
    }
}
