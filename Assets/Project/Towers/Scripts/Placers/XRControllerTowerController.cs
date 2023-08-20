using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class XRControllerTowerController : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    private Tower _selectedTower = null;
    private bool _selecting = false;
    
    [SerializeField]
    [Tooltip("The reference to the action to start the select aiming mode for this controller.")]
    private InputActionReference selectTowerModeActionReference;    
    
    [SerializeField]
    [Tooltip("The reference to the action to confirm tower takeover selection.")]
    private InputActionReference controlTowerConfirmActionReference;

    [SerializeField] private LineRenderer lineRenderer;
    
    private void Start()
    {
        var selectTowerAction = Utilities.GetInputAction(selectTowerModeActionReference);
        if (selectTowerAction != null)
        {
            selectTowerAction.performed += OnStartSelection;
            selectTowerAction.canceled += OnEndSelectMode;
        }
        
        var confirmSelectAction = Utilities.GetInputAction(controlTowerConfirmActionReference);
        if (confirmSelectAction != null)
        {
            confirmSelectAction.performed += OnConfirm;
        }
    }

    private void Update()
    {
        lineRenderer.SetPosition(0, transform.position);
        if(_selecting == false)
        {
            lineRenderer.SetPosition(1, transform.position);
            return;
        }

        SelectATower();
    }

    private void SelectATower()
    {
        var firePointTransform = transform;
        var ray = new Ray(firePointTransform.position, firePointTransform.forward);
        Debug.DrawRay(firePointTransform.position, firePointTransform.forward, Color.magenta);
        
        if (Physics.Raycast(ray, out var hit, 1000, layerMask.value))
        {
            var tower = hit.transform.GetComponent<Tower>();
            if(_selectedTower != tower && _selectedTower)
                _selectedTower.Deselected();
            _selectedTower = tower;
            _selectedTower.Selected();
            // lineRenderer.SetPosition(1, hit.point);
        }
        
        lineRenderer.SetPosition(1, firePointTransform.position + firePointTransform.forward * 100);
    }

    #region Action Event Listeners

    private void OnStartSelection(InputAction.CallbackContext callbackContext)
    {
        _selecting = true;
    }

    private void OnEndSelectMode(InputAction.CallbackContext callbackContext)
    {
        if(_selectedTower)
        {
            _selectedTower.Deselected();
            _selecting = false;
        }
    }

    private void OnConfirm(InputAction.CallbackContext obj)
    {
        if(_selectedTower != null)
        {
            _selectedTower.Deselected();
            PlayerStateController.TakeControlOfTower(_selectedTower);
        }    
    }

    #endregion
}