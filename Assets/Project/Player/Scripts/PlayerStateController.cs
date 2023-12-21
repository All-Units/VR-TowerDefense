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
    [Header("References")]
    [SerializeField] private FadeScreen _fadeScreen;

    [SerializeField] private float fadeAfterDeathTime = 4f;

    public GameObject penthouse;
    public GameObject penthouseInterior;
    public static PlayerControllableTower CurrentTower => instance._currentControlledTower;
    private PlayerControllableTower _currentControlledTower;
    [SerializeField] private bool StartInPenthouse = false;

    private void Awake()
    {
        instance = this;
        SetPlayerState(PlayerState.IDLE);
        ActivatePenthouseExterior();
        if (StartInPenthouse)
            TeleportPlayerToPenthouse();
    }

    public void SetPlayerState(PlayerState newState)
    {
        var prevState = state;
        state = newState;
        
        //Debug.Log($"Setting Player State! {prevState.ToString()} => {state.ToString()}");
        OnStateChange?.Invoke(prevState, state);
    }

    public static void TakeControlOfTower(PlayerControllableTower tower)
    {
        if(IsInstanced() == false) return;
        StartTeleportingReset();
        ActivatePenthouseExterior();
        instance.SetPlayerToTower(tower);
    }

    public void SetPlayerToTower(PlayerControllableTower tower)
    { 
        if(_currentControlledTower != null)
            _currentControlledTower.PlayerReleaseControl();
        
        _currentControlledTower = tower;
        tower.PlayerTakeControl();

        var playerControlPoint = tower.GetPlayerControlPoint();
        
        TeleportPlayerToPoint(playerControlPoint);

        SetPlayerState(PlayerState.TOWER_CONTROL);
        //print($"Set player to {tower.dto.name}");
        dynamicMoveProvider.CanMove = false;
        dynamicMoveProvider.useGravity = false;
    }

    /// <summary>
    /// When a tower dies that is player controlled
    /// </summary>
    public static void DiedInTower()
    {
        ReleaseControlOfTower();
        instance.TeleportPlayerToPenthouse();
        instance._fadeScreen.SetFadeInstant(1);
        instance.StartCoroutine(fadeInAfter());
    }

    static IEnumerator fadeInAfter()
    {
        yield return new WaitForSeconds(instance.fadeAfterDeathTime);
        instance._fadeScreen.FadeIn();
        
    }
    public static void ReleaseControlOfTower()
    {
        if(IsInstanced() == false) return;
        
        instance.ReleasePlayerFromTower();
    }

    private void ReleasePlayerFromTower()
    {
        //print($"Released control of tower");
        var prevTower = _currentControlledTower;
        _currentControlledTower = null;

        playerGameObject.transform.localScale = Vector3.one * normalScale;
        prevTower.PlayerReleaseControl();
        
        SetPlayerState(PlayerState.IDLE);
        dynamicMoveProvider.CanMove = true;
        dynamicMoveProvider.useGravity = true;
        InventoryManager.instance.ReleaseAllItems();
        InventoryManager.HideAllItems();
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
        ActivatePenthouseExterior(false);
        Transform t = TeleportPoints.Penthouse;
        TeleportPlayerToPoint(t);
        
    }

    public void TeleportPlayerToWar()
    {
        ActivatePenthouseExterior();
        Transform t = TeleportPoints.FrontOfGate;
        TeleportPlayerToPoint(t);
        
    }

    private void SetPlayerScale(LocomotionSystem obj)
    {
        playerGameObject.transform.localScale = Vector3.one * (_currentControlledTower == null ? normalScale : towerControlScale);
        teleportationProvider.beginLocomotion -= SetPlayerScale;

        if (_currentControlledTower)
            teleportationProvider.beginLocomotion += OnNextTeleport;
    }

    public static bool IsTeleportingToTower = false;

    private void OnNextTeleport(LocomotionSystem obj)
    {
        teleportationProvider.beginLocomotion -= OnNextTeleport;

        if (IsTeleportingToTower)
        {
            //print($"Was teleporting to tower, not releasing control");
            teleportationProvider.beginLocomotion += OnNextTeleport;
        }
        if (_currentControlledTower && IsTeleportingToTower == false)
        {
            //print($"Had {_currentControlledTower.dto.name}, releasing control");
            ReleaseControlOfTower();
        }
        //IsTeleportingToTower = false;
    }

    private static bool IsInstanced()
    {
        if(instance == null)
            Debug.LogError($"No Player State Controller Detected!");

        return instance != null;
    }

    public static void StartTeleportingReset()
    {
        IsTeleportingToTower = true;
        instance.StartCoroutine(instance._DelayThenResetTeleporting());
    }
    IEnumerator _DelayThenResetTeleporting()
    {
        yield return new WaitForSeconds(0.05f);
        IsTeleportingToTower = false;
    }

    
    /// <summary>
    /// Turns on or off the exterior of the Mages Tower
    /// </summary>
    /// <param name="exterior">If true, exterior on / interior off</param>
    public static void ActivatePenthouseExterior(bool exterior = true)
    {
        // Todo: Handle exception where penthouse is not assigned. Better would be to refactor out to separate class and handle this through the event OnStateChanged
        if (instance.penthouse == null || instance.penthouseInterior == null) return;
        
        instance.penthouse.SetActive(exterior);
        //Set the interior to the inverse of the exterior
        instance.penthouseInterior.SetActive(!exterior);
    }

}
