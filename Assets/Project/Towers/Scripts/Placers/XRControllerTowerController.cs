using UnityEngine;
using UnityEngine.InputSystem;

public class XRControllerTowerController : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    private Tower selectedTower = null;
    private bool _placing = false;
    
    [SerializeField]
    [Tooltip("The reference to the action to start the teleport aiming mode for this controller.")]
    InputActionReference controlTowerModeActivate;
    
    private void Start()
    {
        var placeTowerModeActivateAction = GetInputAction(controlTowerModeActivate);
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

        SelectATower();
    }

    private void SelectATower()
    {
        var firePointTransform = transform;
        var ray = new Ray(firePointTransform.position, firePointTransform.forward);
        if (Physics.Raycast(ray, out var hit, 1000, layerMask.value))
        {
            var tower = hit.transform.GetComponent<Tower>();
        }
    }

    public void OnStartPlacement(InputAction.CallbackContext callbackContext)
    {
        _placing = true;
    }

    public void OnPlaceTower(InputAction.CallbackContext callbackContext)
    {
        if(selectedTower)
            
            _placing = false;
    }
    
    static InputAction GetInputAction(InputActionReference actionReference)
    {
#pragma warning disable IDE0031 // Use null propagation -- Do not use for UnityEngine.Object types
        return actionReference != null ? actionReference.action : null;
#pragma warning restore IDE0031
    }
}