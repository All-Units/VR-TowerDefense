using System.Collections;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

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
    float _MaxHeight = 2f;
    float _heightFromOrigin = 0f;
    public virtual void PlayerTakeControl()
    {
        isPlayerControlled = true;
        StartCoroutine(_KeepPlayerContained());
        onTakeover?.Invoke();
    }

    public virtual void PlayerReleaseControl()
    {
        isPlayerControlled = false;
        onRelease?.Invoke();
    }
    IEnumerator _KeepPlayerContained()
    {
        float origin_y = playerControlPosition.position.y;
        while (isPlayerControlled)
        {
            yield return null;
            float cam_y = InventoryManager.instance.playerCameraTransform.position.y;
            float delta = math.abs(origin_y - cam_y);
            if (delta >= _MaxHeight)
            {
                PlayerStateController.TakeControlOfTower(this);
                yield break;
            }
            _heightFromOrigin = delta;

        }
    }

    public Transform GetPlayerControlPoint()
    {
        return playerControlPosition;
    }
}