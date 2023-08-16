using Project.Towers.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class XRControllerTowerPlacer : MonoBehaviour
{
    [SerializeField] private Tower_SO towerToPlace;
    [SerializeField] private LayerMask layerMask;
    private MapTile selectedTile = null;
    private bool _placing = false;
    
    [SerializeField]
    [Tooltip("The reference to the action to start the teleport aiming mode for this controller.")]
    InputActionReference placeTowerModeActivate;
    
    private void Start()
    {
        var placeTowerModeActivateAction = GetInputAction(placeTowerModeActivate);
        if (placeTowerModeActivateAction != null)
        {
            //Debug.Log("Found Action!");
            placeTowerModeActivateAction.performed += OnStartPlacement;
            placeTowerModeActivateAction.canceled += OnPlaceTower;
        }
    }

    private void Update()
    {
        if(_placing == false) return;

        SelectATile();
    }

    private void SelectATile()
    {
        var firePointTransform = transform;
        var ray = new Ray(firePointTransform.position, firePointTransform.forward);
        if (Physics.Raycast(ray, out var hit, 1000, layerMask.value))
        {
            var tile = hit.transform.GetComponent<MapTile>();

            if (tile != selectedTile && tile.selectable)
            {
                TowerSpawnManager.Instance.PlaceGhost(tile.transform.position);
                selectedTile = tile;
            }
        }
    }

    public void OnStartPlacement(InputAction.CallbackContext callbackContext)
    {
        //Debug.Log("Starting Placement!");
        _placing = true;
    }

    public void OnPlaceTower(InputAction.CallbackContext callbackContext)
    {
        if(selectedTile)
            TowerSpawnManager.Instance.PlaceTower(selectedTile.transform.position);
       
        _placing = false;
    }
    
    static InputAction GetInputAction(InputActionReference actionReference)
    {
#pragma warning disable IDE0031 // Use null propagation -- Do not use for UnityEngine.Object types
        return actionReference != null ? actionReference.action : null;
#pragma warning restore IDE0031
    }
}