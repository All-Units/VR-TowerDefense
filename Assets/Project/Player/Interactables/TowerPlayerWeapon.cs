using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TowerPlayerWeapon : MonoBehaviour
{
    [SerializeField] private PlayerItem_SO data;
    public PlayerItem_SO Data => data;

    public PlayerPower Power;
    public static Action<TowerPlayerWeapon, Enemy> onKill;
    /// <summary>
    /// Gets the attached game object for the given player weapon
    /// <para>Returns the Power throwable if there is a power</para>
    /// <para>If power is null, returns the projectile ammo</para>
    /// </summary>
    public GameObject GetGameObject
    {
        get
        {
            if (Power != null)
            {
                return Power.throwable.gameObject;
            }
            else if (data != null)
            {
                return data.ammo.gameObject;
            }
            return null;
        }
    }
    public DamageDealer GetDamageDealer => GetGameObject?.GetComponent<DamageDealer>();
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
