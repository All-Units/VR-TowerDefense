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
        //StartCoroutine(_KeepPlayerContained());
        onTakeover?.Invoke();
    }

    public virtual void PlayerReleaseControl()
    {
        isPlayerControlled = false;
        onRelease?.Invoke();
        print($"Player released control");
    }
    /*
    IEnumerator _KeepPlayerContained()
    {
        float origin_y = playerControlPosition.position.y;
        //yield break;
        while (isPlayerControlled)
        {
            yield return null;
            float cam_y = InventoryManager.instance.playerCameraTransform.position.y;
            float delta = cam_y - origin_y;
            if (delta >= _MaxHeight)
            {
                yield return new WaitForSeconds(0.1f);
                
                print($"Height was too high! {delta} above. Is player controlled? {isPlayerControlled}. Current tower is this? {PlayerStateController.CurrentTower == this}");
                if (isPlayerControlled == false || PlayerStateController.CurrentTower != this) yield break;

                //PlayerStateController.TakeControlOfTower(this);
                yield break;
            }
            _heightFromOrigin = delta;

        }
    }*/

    public Transform GetPlayerControlPoint()
    {
        return playerControlPosition;
    }
}