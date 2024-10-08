using System.Collections;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class PlayerControllableTower : Tower
{
    /// <summary>
    /// If the tower is currently controlled by the player
    /// </summary>
    public bool isPlayerControlled { get; private set; } = false;
    [SerializeField] private Transform playerControlPosition;
    
    public UnityEvent onTakeover;
    public UnityEvent onRelease;

    public override void Die()
    {
        base.Die();
        if (isPlayerControlled)
        {
            PlayerStateController.DiedInTower();
            
            PlayerReleaseControl();
            InventoryManager.instance.ReleaseAllItems();
        }
        
        
    }
    public virtual void PlayerTakeControl()
    {
        isPlayerControlled = true;
        onTakeover?.Invoke();
    }

    public virtual void PlayerReleaseControl()
    {
        isPlayerControlled = false;
        onRelease?.Invoke();
    }
    

    public Transform GetPlayerControlPoint()
    {
        return playerControlPosition;
    }
}