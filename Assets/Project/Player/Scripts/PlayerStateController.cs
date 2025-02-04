using System;
using System.Collections;
using UnityEngine;
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
    public Transform frontGate;
    public GameObject penthouseInterior;
    public static PlayerControllableTower CurrentTower => instance._currentControlledTower;
    private PlayerControllableTower _currentControlledTower;
    private bool _joiningTower;
    [SerializeField] private bool StartInPenthouse = false;

    public Action<PlayerControllableTower> OnPlayerTakeoverTower;
    public Action<PlayerControllableTower> OnPlayerQuickTakeoverTower;
    public Action OnPlayerReleaseTower;

    private void Awake()
    {
        instance = this;
        SetPlayerState(PlayerState.IDLE);
        ActivatePenthouseExterior();
        if (StartInPenthouse)
            TeleportPlayerToPenthouse();
    }

    private void Start()
    {
        teleportationProvider.beginLocomotion += OnNextTeleport;
    }

    public void SetPlayerState(PlayerState newState)
    {
        var prevState = state;
        state = newState;
        
        //Debug.Log($"Setting Player State! {prevState.ToString()} => {state.ToString()}");
        OnStateChange?.Invoke(prevState, state);
    }
    public static void QuickTakeover(PlayerControllableTower tower)
    {
        TakeControlOfTower(tower);
        if (instance == null) return;
        instance.OnPlayerQuickTakeoverTower?.Invoke(tower);
    }

    public static void TakeControlOfTower(PlayerControllableTower tower)
    {
        if(IsInstanced() == false) return;
        ActivatePenthouseExterior();
        instance.SetPlayerToTower(tower);
    }

    private void SetPlayerToTower(PlayerControllableTower tower)
    { 
        if(_currentControlledTower != null)
            _currentControlledTower.PlayerReleaseControl();
        OnPlayerTakeoverTower.Invoke(tower);
        _currentControlledTower = tower;
        tower.PlayerTakeControl();
        _joiningTower = true;
        
        var playerControlPoint = tower.GetPlayerControlPoint();
        Vector3 dir = new Vector3(0f, InventoryManager.instance.playerCameraTransform.eulerAngles.y, 0f);
        playerControlPoint.transform.eulerAngles = dir;
        _TeleportPlayerToPoint(playerControlPoint);
        
        SetPlayerState(PlayerState.TOWER_CONTROL);

        dynamicMoveProvider.CanMove = false;
        dynamicMoveProvider.useGravity = false;
    }

    /// <summary>
    /// When a tower dies that is player controlled
    /// </summary>
    public static void DiedInTower()
    {
        ReleaseControlOfTower();
        instance.TeleportPlayerToWar();
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

        OnPlayerReleaseTower?.Invoke();
    }

    public static void TeleportPlayerToPoint(Transform transform)
    {
        if(transform == null) return;
        
        TeleportPlayerToPoint(transform.position, transform.rotation);
    }
    
    public static void TeleportPlayerToPoint(Vector3 pos, Quaternion rot)
    {
        if (IsInstanced() == false) return;
        TeleportRequest request = new TeleportRequest()
        {
            requestTime = Time.time,
            matchOrientation = MatchOrientation.TargetUpAndForward,

            destinationPosition = pos,
            destinationRotation = rot
        };

        instance.teleportationProvider.QueueTeleportRequest(request);
    }
    private void _TeleportPlayerToPoint(Transform playerControlPoint)
    {
        TeleportRequest request = new TeleportRequest()
        {
            requestTime = Time.time,
            matchOrientation = MatchOrientation.TargetUpAndForward,

            destinationPosition = playerControlPoint.position,
            destinationRotation = playerControlPoint.rotation
        };

        teleportationProvider.QueueTeleportRequest(request);
    }

    public void TeleportPlayerToPenthouse()
    {
        ActivatePenthouseExterior(false);
        Transform t = TeleportPoints.Penthouse;
        _TeleportPlayerToPoint(t);
    }

    public void TeleportPlayerToWar()
    {
        ActivatePenthouseExterior();
        Transform t = Gate.FrontGate;
        _TeleportPlayerToPoint(t);
    }
    
    private void OnNextTeleport(LocomotionSystem obj)
    {
        TeleportFloorPlacer.ManualRefresh();
        if (_joiningTower)
        {
            _joiningTower = false;
            return;
        }

        if (_currentControlledTower)
        {
            ReleaseControlOfTower();
        }
    }

    private static bool IsInstanced()
    {
        if(instance == null)
            Debug.LogError($"No Player State Controller Detected!");

        return instance != null;
    }
    public static TeleportationProvider teleporter
    {
        get
        {
            if (instance == null) return null;
            if (instance._tp == null)
                instance._tp = instance.GetComponentInChildren<TeleportationProvider>();
            return instance._tp;
        }
    }
    TeleportationProvider _tp = null;

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
