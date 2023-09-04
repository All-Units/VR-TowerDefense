using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

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
    public XRDirectInteractor playerHand;

    
    
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
        
        PlayerStateController.OnStateChange += PlayerStateControllerOnStateChange;
    }

    private void PlayerStateControllerOnStateChange(PlayerState arg1, PlayerState arg2)
    {
        switch (arg2)
        {
            case PlayerState.IDLE:
                lineRenderer.enabled = true;
                break;
            case PlayerState.TOWER_CONTROL:
                lineRenderer.enabled = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(arg2), arg2, null);
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

        if(PlayerStateController.instance.state == PlayerState.IDLE) 
            SelectATower();
    }

    private void SelectATower()
    {
        var firePointTransform = transform;
        var ray = new Ray(firePointTransform.position, firePointTransform.forward);
        
        if (Physics.Raycast(ray, out var hit, 1000, layerMask.value))
        {
            var tower = hit.transform.GetComponent<Tower>();
            if (tower)
            {
                if(_selectedTower != tower && _selectedTower)
                    _selectedTower.Deselected();
                _selectedTower = tower;
                _selectedTower.Selected();
            }
        }
        
        lineRenderer.SetPosition(1, firePointTransform.position + firePointTransform.forward * 100);
    }

    #region Action Event Listeners

    private void OnStartSelection(InputAction.CallbackContext callbackContext)
    {
        if(playerHand.interactablesSelected.Any() == false)
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
            _selectedTower = null;
            _selecting = false;
        }    
    }

    #endregion
}