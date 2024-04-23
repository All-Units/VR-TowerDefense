using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[CreateAssetMenu(menuName = "SO/Tower Takeover/Player Item", fileName = "New Item")]
public class PlayerItem_SO : TowerTakeoverObject
{
    public enum ItemAmmoPouch
    {
        NONE = -1,
        ARROW_QUIVER,
        BOMB_SATCHEL
    }
    
    public TowerPlayerWeapon itemGo;
    public ItemAmmoPouch ammoPouch = ItemAmmoPouch.NONE;
    public XRGrabInteractable ammo;

    public bool holdInOffHand;
    
    #region HelperFunctions

    public bool HasItemPouch() => ammoPouch != ItemAmmoPouch.NONE;

    #endregion
}

public abstract class TowerTakeoverObject : ScriptableObject
{
    public static Action<TowerTakeoverObject> OnKillWithItem;
}