using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public enum PlayerState
{
    IDLE = 0,
    TOWER_CONTROL = 1
}

public class PlayerStateController : MonoBehaviour
{
    public static PlayerStateController instance;

    public PlayerState state { get; private set; } = PlayerState.IDLE;

    /// <summary>
    /// On State Change event. Fired only internally by state manager. Params: (PlayerState OldState, PlayerState NewState)
    /// </summary>
    public static event Action<PlayerState, PlayerState> OnStateChange;

    [SerializeField] private GameObject playerGameObject;
    
    [Header("Scale Modifiers")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float towerControlScale;
    [SerializeField] private TeleportationProvider teleportationProvider;
    [SerializeField] private DynamicMoveProvider dynamicMoveProvider;
    
    private Tower _currentControlledTower;

    private void Awake()
    {
        instance = this;
        SetPlayerState(PlayerState.IDLE);
    }

    public void SetPlayerState(PlayerState newState)
    {
        var prevState = state;
        state = newState;
        
        //Debug.Log($"Setting Player State! {prevState.ToString()} => {state.ToString()}");
        OnStateChange?.Invoke(prevState, state);
    }

    public static void TakeControlOfTower(Tower tower)
    {
        if(IsInstanced() == false) return;
        
        instance.SetPlayerToTower(tower);
    }

    public void SetPlayerToTower(Tower tower)
    { 
        if(_currentControlledTower != null)
            _currentControlledTower.PlayerReleaseControl();
        
        _currentControlledTower = tower;
        tower.PlayerTakeControl();

        var playerControlPoint = tower.GetPlayerControlPoint();
        
        TeleportPlayerToPoint(playerControlPoint);

        SetPlayerState(PlayerState.TOWER_CONTROL);
        dynamicMoveProvider.CanMove = false;
        dynamicMoveProvider.useGravity = false;
    }

    public static void ReleaseControlOfTower()
    {
        if(IsInstanced() == false) return;
        
        instance.ReleasePlayerFromTower();
    }

    private void ReleasePlayerFromTower()
    {
        var prevTower = _currentControlledTower;
        _currentControlledTower = null;

        playerGameObject.transform.localScale = Vector3.one * normalScale;
        prevTower.PlayerReleaseControl();
        
        SetPlayerState(PlayerState.IDLE);
        dynamicMoveProvider.CanMove = true;
        dynamicMoveProvider.useGravity = true;
        InventoryManager.instance.ReleaseAllItems();
    }
    
    private void TeleportPlayerToPoint(Transform playerControlPoint)
    {
        TeleportRequest request = new TeleportRequest()
        {
            requestTime = Time.time,
            matchOrientation = MatchOrientation.TargetUpAndForward,

            destinationPosition = playerControlPoint.position,
            destinationRotation = playerControlPoint.rotation
        };

        teleportationProvider.QueueTeleportRequest(request);

        teleportationProvider.beginLocomotion += SetPlayerScale;
    }

    public void TeleportPlayerToPenthouse()
    {
        Transform t = TeleportPoints.Penthouse;
        TeleportPlayerToPoint(t);
    }

    private void SetPlayerScale(LocomotionSystem obj)
    {
        playerGameObject.transform.localScale = Vector3.one * (_currentControlledTower == null ? normalScale : towerControlScale);
        teleportationProvider.beginLocomotion -= SetPlayerScale;

        if (_currentControlledTower)
            teleportationProvider.beginLocomotion += OnNextTeleport;
    }

    private void OnNextTeleport(LocomotionSystem obj)
    {
        teleportationProvider.beginLocomotion -= OnNextTeleport;

        if(_currentControlledTower)
            ReleaseControlOfTower();
    }

    private static bool IsInstanced()
    {
        if(instance == null)
            Debug.LogError($"No Player State Controller Detected!");

        return instance != null;
    }

}
