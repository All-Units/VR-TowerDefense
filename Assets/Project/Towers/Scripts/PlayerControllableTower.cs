using UnityEngine;
using UnityEngine.Events;

public class PlayerControllableTower : Tower
{
    public bool isPlayerControlled { get; private set; } = false;
    [SerializeField] private Transform playerControlPosition;
    
    public UnityEvent onTakeover;
    public UnityEvent onRelease;

    public override void Die()
    {
        if (isPlayerControlled)
        {
            PlayerStateController.DiedInTower();
            
            PlayerReleaseControl();
            InventoryManager.instance.ReleaseAllItems();
        }
        
        base.Die();
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