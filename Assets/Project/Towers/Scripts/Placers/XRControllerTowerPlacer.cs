using System;
using Project.Towers.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

public class XRControllerTowerPlacer : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private LineRenderer _lr;
    [SerializeField] private Transform firePoint;
    public bool placing { get; private set; }= false;
    [SerializeField] private float width = 0.5f;

    [SerializeField] private Material validColor;
    [SerializeField] private Material invalidColor;

    private BaseItem parentItem;

    public Inventory2 inv;
    
    [SerializeField]
    [Tooltip("The reference to the action to confirm tower takeover selection.")]
    private InputActionReference placeTowerActionReference;

    public event Action OnPlaceTowerEvent;

    private void Start()
    {
        var placeTowerAction = Utilities.GetInputAction(placeTowerActionReference);
        if (placeTowerAction != null)
        {
            placeTowerAction.started += OnPlaceTower;
        }   
    }

    public void Pickup()
    {
        inv.trigger.action.performed += OnStartPlacement;
        inv.trigger.action.canceled += OnPlaceTower;
    }

    public void Drop()
    {
        inv.trigger.action.performed -= OnStartPlacement;
        inv.trigger.action.canceled -= OnPlaceTower;
        Close();
    }

    private void Update()
    {
        clearRay();
        if(placing == false) return;
        SelectATile();
    }

    private Vector3 lastTowerPos = Vector3.negativeInfinity;
    [SerializeField] private int maxDistance = 50;

    private void SelectATile()
    {
        var firePointTransform = firePoint;
        Vector3 pos = firePointTransform.position;
        Vector3 forward = firePointTransform.forward;
        var ray = new Ray(pos, forward);
        if (Physics.Raycast(ray, out var hit, maxDistance, layerMask.value))
        {
            bool valid = true;
            valid = valid && (hit.transform.gameObject.layer == 7);
            Vector3 hitPos = hit.point;
            if (valid && lastTowerPos != hitPos)
            {
                TowerSpawnManager.Instance.PlaceGhost(hitPos, transform.position);
                lastTowerPos = hitPos;
            }
            DrawRay(pos, hit.point);
            if (TowerSpawnManager.CouldAffordCurrentTower == false)
                TowerSpawnManager.Instance.HideGhost();
        }
        else
        {
            lastTowerPos = Vector3.negativeInfinity;
            TowerSpawnManager.Instance.HideGhost();
            DrawRay(pos, pos + (forward * 100f), false);
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
        placing = true;
    }

    public void Close()
    {
        placing = false;
        clearRay();
    }

    public void OnPlaceTower(InputAction.CallbackContext callbackContext)
    {
        if (placing == false) return;
        
        if (lastTowerPos.y > -100000f)
        {
            TowerSpawnManager.Instance.PlaceTower(lastTowerPos);
        }
       
        placing = false;
        
        OnPlaceTowerEvent?.Invoke();
    }

    public void PlaceOne()
    {
        placing = true;
    }
}