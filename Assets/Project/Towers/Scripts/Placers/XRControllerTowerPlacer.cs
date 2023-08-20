using Project.Towers.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class XRControllerTowerPlacer : MonoBehaviour
{
    [SerializeField] private Tower_SO towerToPlace;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private LineRenderer _lr;
    [SerializeField] private Transform firePoint;
    private MapTile selectedTile = null;
    private bool _placing = false;
    [SerializeField] private float width = 0.5f;
    
    [SerializeField]
    [Tooltip("The reference to the action to start the teleport aiming mode for this controller.")]
    InputActionReference placeTowerModeActivate;

    [SerializeField] private Material validColor;
    [SerializeField] private Material invalidColor;

    private BaseItem parentItem;
    private void Start()
    {
        parentItem = GetComponentInParent<BaseItem>();
        //var placeTowerModeActivateAction = GetInputAction(placeTowerModeActivate);
        var placeTowerModeActivateAction = GetInputAction(parentItem.trigger);
        if (placeTowerModeActivateAction != null)
        {
            //Debug.Log("Found Action!");
            placeTowerModeActivateAction.performed += OnStartPlacement;
            placeTowerModeActivateAction.canceled += OnPlaceTower;
        }
    }

    private void Update()
    {
        clearRay();
        if(_placing == false) return;
        SelectATile();
    }

    private void SelectATile()
    {
        var firePointTransform = firePoint;
        Vector3 pos = firePointTransform.position;
        Vector3 forward = firePointTransform.forward;
        var ray = new Ray(pos, forward);
        if (Physics.Raycast(ray, out var hit, 1000, layerMask.value))
        {
            var tile = hit.transform.GetComponent<MapTile>();

            if (tile != selectedTile && tile.selectable)
            {
                TowerSpawnManager.Instance.PlaceGhost(tile.transform.position);
                selectedTile = tile;
            }
            DrawRay(pos, hit.transform.position);
            if (TowerSpawnManager.CouldAffordCurrentTower == false)
                TowerSpawnManager.Instance.HideGhost();
        }
        else
        {
            selectedTile = null;
            TowerSpawnManager.Instance.HideGhost();
            DrawRay(pos, forward * 100f, false);
        }
        
    }

    void clearRay()
    {
        _lr.SetPosition(0, Vector3.zero);
        _lr.SetPosition(1, Vector3.zero);
    }
    void DrawRay(Vector3 start, Vector3 end, bool valid = true)
    {
        if (valid && TowerSpawnManager.CouldAffordCurrentTower)
            _lr.material = validColor;
        else
            _lr.material = invalidColor;
        _lr.SetPosition(0, start);
        _lr.SetPosition(1, end);
        _lr.startWidth = width;
        _lr.endWidth = width;

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