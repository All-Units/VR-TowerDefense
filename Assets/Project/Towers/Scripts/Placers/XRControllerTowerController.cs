using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class XRControllerTowerController : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    private Tower _selectedTower = null;
    private bool _selecting = true;
    
    [SerializeField]
    [Tooltip("The reference to the action to start the select aiming mode for this controller.")]
    private InputActionReference selectTowerModeActionReference;    
    
    [SerializeField]
    [Tooltip("The reference to the action to confirm tower takeover selection.")]
    private InputActionReference controlTowerConfirmActionReference; 
    
    [SerializeField]
    [Tooltip("The reference to the action to confirm tower takeover selection.")]
    private InputActionReference openTowerMenuActionReference;

    public XRDirectInteractor playerHand;
    private List<XRBaseInteractor> otherInteractors;
    
    private void Start()
    {
        otherInteractors = transform.parent.GetComponentsInChildren<XRBaseInteractor>().ToList();
        
        var openTowerMenuAction = Utilities.GetInputAction(openTowerMenuActionReference);
        if (openTowerMenuAction != null)
        {
            openTowerMenuAction.started += OpenTowerMenuActionOnStarted;
            openTowerMenuAction.canceled += OpenTowerMenuActionOnCanceled;
        }
        
        PlayerStateController.OnStateChange += PlayerStateControllerOnStateChange;
    }

    private void PlayerStateControllerOnStateChange(PlayerState arg1, PlayerState arg2)
    {
        
    }

    private void Update()
    {
        if(otherInteractors.Any(tor=> tor.interactablesSelected.Any()))
            return;

        SelectATower();
    }
    
    private void SelectATower()
    {
        var firePointTransform = transform;

        var ray = new Ray(firePointTransform.position, firePointTransform.forward);
        Vector3 point = firePointTransform.position + firePointTransform.forward * 100;
        if (Physics.SphereCast(ray, .33f, out var hit, 1000, layerMask.value))
        {
            var tower = hit.transform.GetComponent<Tower>();
            if (tower)
            {
                if(_selectedTower != tower && _selectedTower)
                    _selectedTower.Deselected();
                _selectedTower = tower;
                _selectedTower.Selected();
            }
            
            //We hit something that isn't a tower
            else
            {
                _deselectCurrent();
            }

            point = hit.point;
        }
        else
        {
            _deselectCurrent();
        }
    }

    void _deselectCurrent()
    {
        if (_selectedTower != null)
        {
            _selectedTower.Deselected();
            _selectedTower = null;
        }
    }

    private Coroutine _actionButtonCoroutine;
    [SerializeField] private float timeToQuickTakeOver = 1f;

    private void OpenTowerMenuActionOnStarted(InputAction.CallbackContext obj)
    {
        if (_actionButtonCoroutine == null)
            _actionButtonCoroutine = StartCoroutine(ActionButtonCoroutine());

    }
    
    private void OpenTowerMenuActionOnCanceled(InputAction.CallbackContext obj)
    {
        if(_actionButtonCoroutine != null)
        {
            if(_selectedTower != null)
                BubbleMenuController.Open(_selectedTower);
            
            StopCoroutine(_actionButtonCoroutine);
            _actionButtonCoroutine = null;
        }    
    }

    private IEnumerator ActionButtonCoroutine()
    {
        var t = timeToQuickTakeOver;
        while (t > 0)
        {
            yield return null;
            t -= Time.deltaTime;
        }
        
        OnConfirm();
        _actionButtonCoroutine = null;
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

    public void StartSelection(InputAction.CallbackContext context)
    {
        _selecting = true;
    }

    public void EndSelection(InputAction.CallbackContext context)
    {
        if (_selectedTower != null)
        {
            _selectedTower.Deselected();
            
            PlayerStateController.TakeControlOfTower(_selectedTower);
            _selectedTower = null;
        }
        
        _selecting = false;
    }

    private void OnConfirm()
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