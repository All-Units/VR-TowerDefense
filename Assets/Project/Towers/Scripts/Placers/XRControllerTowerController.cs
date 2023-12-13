using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Towers.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// TODO: Rework this and the tower placer to a state machine
/// </summary>

public class XRControllerTowerController : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    private Tower _selectedTower = null;
    private bool _selecting = true;
    
    [SerializeField]
    [Tooltip("The reference to the action to confirm tower takeover selection.")]
    private InputActionReference openTowerMenuActionReference;
    
    [SerializeField]
    [Tooltip("The reference to the action to confirm tower takeover selection.")]
    private InputActionReference openTowerPlacementActionReference;    
    
    [SerializeField]
    [Tooltip("The reference to the action to confirm tower takeover selection.")]
    private InputActionReference closeTowerPlacementActionReference; 

    public XRDirectInteractor playerHand;
    private List<XRBaseInteractor> otherInteractors;

    [SerializeField] private List<Tower_SO> availableTowers;
    [SerializeField] private BubbleMenuOption towersBubblePrefab;
    [SerializeField] private BubbleMenuOption currencyBubblePrefab;
    [SerializeField] private Transform towersBubbleRoot;
    [SerializeField] private float bubbleMenuRadius;
    [SerializeField] private float bubbleMenuMaxAngle;
    [SerializeField] private XRControllerTowerPlacer towerPlacer;

    private bool _selectorLock = false;
    
    private void Start()
    {
        otherInteractors = transform.parent.GetComponentsInChildren<XRBaseInteractor>().ToList();
        
        var openTowerMenuAction = Utilities.GetInputAction(openTowerMenuActionReference);
        if (openTowerMenuAction != null)
        {
            openTowerMenuAction.started += OpenTowerMenuActionOnStarted;
            openTowerMenuAction.canceled += OpenTowerMenuActionOnCanceled;
        }   
        
        var openTowerPlacementBubbleAction = Utilities.GetInputAction(openTowerPlacementActionReference);
        if (openTowerPlacementBubbleAction != null)
        {
            openTowerPlacementBubbleAction.started += OpenTowerBubbles;
        }        
        
        var closeTowerPlacementBubbleAction = Utilities.GetInputAction(closeTowerPlacementActionReference);
        if (closeTowerPlacementBubbleAction != null)
        {
            closeTowerPlacementBubbleAction.started += CloseTowerBubbles;
        }

        towerPlacer.OnPlaceTowerEvent += OnPlace;
    }
    
    private void Update()
    {
        if(otherInteractors.Any(tor=> tor.interactablesSelected.Any()) || towerPlacer.placing || _selectorLock)
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
        if(towerPlacer.placing) return;
        
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

    private void OpenTowerBubbles(InputAction.CallbackContext obj)
    {
        if(otherInteractors.Any(tor=> tor.interactablesSelected.Any()) || towerPlacer.placing || _selectorLock)
            return;
        
        towersBubbleRoot.DestroyChildren();
        towersBubbleRoot.position = transform.position;
        towersBubbleRoot.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        var step = bubbleMenuMaxAngle / (availableTowers.Count - 1);
        var mainTransform = Camera.main.transform;

        for (var i = 0; i < availableTowers.Count; i++)
        {
            var angle = i * step - (bubbleMenuMaxAngle / 2f);
            var position = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * bubbleMenuRadius;

            var newBubble = Instantiate(towersBubblePrefab, towersBubbleRoot);
            newBubble.transform.localPosition = position;
            newBubble.transform.LookAt(mainTransform, Vector3.up);
            var towerSo = availableTowers[i];
            var icon = Instantiate(towerSo.iconPrefab, newBubble.transform);
            icon.transform.position += Vector3.down * .07f;
            icon.transform.rotation = Quaternion.identity;
            icon.transform.localScale *= 2;
            newBubble.Initialize(()=>SetTowerToPlace(towerSo), towerSo.name, towerSo.cost);
        }
        
        var currencyBubble = Instantiate(currencyBubblePrefab, towersBubbleRoot);
        currencyBubble.Initialize(null, $"${CurrencyManager.CurrentCash}");
        currencyBubble.transform.localPosition = new Vector3(0, -bubbleMenuRadius / 3f, bubbleMenuRadius / 2f);
        currencyBubble.transform.LookAt(mainTransform, Vector3.up);
        
        towersBubbleRoot.SetParent(null);
    }

    private void CloseTowerBubbles(InputAction.CallbackContext obj)
    {
        towersBubbleRoot.DestroyChildren();
        towerPlacer.Close();
    }

    private void SetTowerToPlace(Tower_SO so)
    {
        TowerSpawnManager.SetTower(so);
        towerPlacer.PlaceOne();
        towersBubbleRoot.DestroyChildren();
    }

    public void OnPlace()
    {
        StartCoroutine(SelectCooldown());
    }

    private IEnumerator SelectCooldown()
    {
        _selectorLock = true;
        yield return new WaitForSeconds(1);
        _selectorLock = false;
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