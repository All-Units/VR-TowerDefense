using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class MovementLocker : MonoBehaviour
{
    private whichHand hand => inventory.hand;
    public Inventory inventory;
    /// <summary>
    /// Can enable / disable movement
    /// </summary>
    public ActionBasedControllerManager acbm;
    
    
    /// <summary>
    /// Places a movement lock, depending on the hand the item resides in
    /// </summary>
    public virtual void PlaceMovementLock(GameObject caller)
    {
        if (hand == whichHand.left)
            PlaceMovementLockLeft();
        else 
            PlaceMovementLockRight(caller);
    }
    /// <summary>
    /// Removes the correct movement lock from the correct hand
    /// </summary>
    public virtual void RemoveMovementLock(GameObject caller)
    {
        if (hand == whichHand.left)
            RemoveMovementLockLeft();
        else RemoveMovementLockRight(caller);
    }

    public virtual void PlaceMovementLockLeft()
    {
        DynamicMoveProvider.AddMovementLock();
    }

    private HashSet<GameObject> lockedObjs = new HashSet<GameObject>();
    public virtual void PlaceMovementLockRight(GameObject caller)
    {
        //If the object calling the lock is not already locking movement
        if (lockedObjs.Contains(caller) == false)
        {
            acbm.FreezeMovement();
            lockedObjs.Add(caller); 
            print($"Freezing movement");
        }
        //Else do nothing
    }

    public virtual void RemoveMovementLockLeft()
    {
        DynamicMoveProvider.RemoveMovementLock();
    }

    public virtual void RemoveMovementLockRight(GameObject caller)
    {
        //Only do anything if the obj is locked
        if (lockedObjs.Contains(caller))
        {
            acbm.EnableLocomotionActions();
            lockedObjs.Remove(caller);
        }
    }
}
