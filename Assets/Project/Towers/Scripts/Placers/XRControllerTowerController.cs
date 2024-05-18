using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Towers.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
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
    InputAction openTowerMenuAction => Utilities.GetInputAction(openTowerMenuActionReference);
    
    [SerializeField]
    [Tooltip("The reference to the action to confirm tower takeover selection.")]
    private InputActionReference openTowerPlacementActionReference;  
    InputAction openTowerPlacementBubbleAction => Utilities.GetInputAction(openTowerPlacementActionReference);
    
    [SerializeField]
    [Tooltip("The reference to the action to confirm tower takeover selection.")]
    private InputActionReference closeTowerPlacementActionReference; 
    InputAction closeTowerPlacementBubbleAction => Utilities.GetInputAction(closeTowerPlacementActionReference);

    public XRDirectInteractor playerHand;
    private List<XRBaseInteractor> otherInteractors;

    [SerializeField] private List<Tower_SO> availableTowers;
    [SerializeField] private BubbleMenuOption towersBubblePrefab;
    [SerializeField] private BubbleMenuOption currencyBubblePrefab;
    [SerializeField] private Transform towersBubbleRoot;
    [SerializeField] private float bubbleMenuRadius;
    [SerializeField] private float bubbleMenuCashHeight = -1f;
    [SerializeField] private float bubbleMenuMaxAngle;
    [SerializeField] private XRControllerTowerPlacer towerPlacer;

    private bool _selectorLock = false;

    [SerializeField] float deselectGracePeriod = 1f;

    public static XRControllerTowerController instance;
    
    private void Start()
    {
        instance = this;
        otherInteractors = transform.parent.GetComponentsInChildren<XRBaseInteractor>().ToList();
        
        if (openTowerMenuAction != null)
        {
            openTowerMenuAction.started += OpenTowerMenuActionOnStarted;
            openTowerMenuAction.canceled += OpenTowerMenuActionOnCanceled;
        }   
        
        if (openTowerPlacementBubbleAction != null)
        {
            openTowerPlacementBubbleAction.started += OpenTowerBubbles;
        }        
        
        if (closeTowerPlacementBubbleAction != null)
        {
            closeTowerPlacementBubbleAction.started += CloseTowerBubbles;
        }

        towerPlacer.OnPlaceTowerEvent += OnPlace;
        XRPauseMenu.OnPause += _CloseBubbles;
    }
    
    private void Update()
    {
        if (XRPauseMenu.IsPaused) return;
        if (BubbleMenuOption.IsCurrentlyHovering) return;
        if (otherInteractors.Any(tor=> tor.interactablesSelected.Any()) || towerPlacer.placing || _selectorLock)
            return;

        SelectATower();
    }
    private void OnDestroy()
    {
        XRPauseMenu.OnPause -= _CloseBubbles;

        if (openTowerMenuAction != null)
        {
            openTowerMenuAction.started -= OpenTowerMenuActionOnStarted;
            openTowerMenuAction.canceled -= OpenTowerMenuActionOnCanceled;
        }

        if (openTowerPlacementBubbleAction != null)
        {
            openTowerPlacementBubbleAction.started -= OpenTowerBubbles;
        }

        if (closeTowerPlacementBubbleAction != null)
        {
            closeTowerPlacementBubbleAction.started -= CloseTowerBubbles;
        }

    }

    private void SelectATower()
    {
        var firePointTransform = transform;

        var ray = new Ray(firePointTransform.position, firePointTransform.forward);
        Vector3 point = firePointTransform.position + firePointTransform.forward * 100;
        if (Physics.SphereCast(ray, .5f, out var hit, 100, layerMask.value))
        {
            var tower = hit.transform.GetComponent<Tower>();
            if(tower is PlayerControllableTower { isPlayerControlled: true }) return;
            if (tower && tower.IsInitialized == false) return;
            if (tower)
            {
                if(_selectedTower != tower && _selectedTower)
                    _selectedTower.Deselected();
                _selectedTower = tower;
                _selectedTower.Selected();

                _StopDeselectDelay();
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

    IEnumerator _currentDeselector = null;
    void _deselectCurrent()
    {
        if (_currentDeselector != null) return;
        _currentDeselector = _DelayDeselect();
        StartCoroutine(_currentDeselector);
        
    }
    void _StopDeselectDelay()
    {
        if (_currentDeselector != null)
            StopCoroutine(_currentDeselector);
        _currentDeselector = null;
    }
    IEnumerator _DelayDeselect()
    {
        yield return new WaitForSeconds(deselectGracePeriod);
        if (_selectedTower != null)
        {
            _selectedTower.Deselected();
            _selectedTower = null;
        }
        _currentDeselector = null;
    }

    private Coroutine _actionButtonCoroutine;
    [SerializeField] private float timeToQuickTakeOver = 1f;

    private void OpenTowerMenuActionOnStarted(InputAction.CallbackContext obj)
    {
        if (XRPauseMenu.IsPaused) return;
        if (towerPlacer.placing) return;
        
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
        if (XRPauseMenu.IsPaused) return;
        //print($"Trying to open tower bubbles, other interactors? {otherInteractors.Any(tor => tor.interactablesSelected.Any())}, " +
        //    $"tower placing? {towerPlacer.placing}, _selectorLock? {_selectorLock}");
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
            newBubble.Initialize(()=>SetTowerToPlace(towerSo), towerSo.name, towerSo.cost, towerSo.description);
        }
        
        var currencyBubble = Instantiate(currencyBubblePrefab, towersBubbleRoot);
        currencyBubble.Initialize(null, $"${CurrencyManager.CurrentCash}");
        currencyBubble.transform.localPosition = new Vector3(0, bubbleMenuCashHeight, bubbleMenuRadius / 2f);
        currencyBubble.transform.LookAt(mainTransform, Vector3.up);
        
        towersBubbleRoot.SetParent(null);
        DeselectCurrent();
        BubbleMenuController.Hide();
    }

    public static void _CloseBubbles()
    {
        if (instance == null) return;
        instance.CloseTowerBubbles(new InputAction.CallbackContext());
    }
    private void CloseTowerBubbles(InputAction.CallbackContext obj)
    {
        towersBubbleRoot.DestroyChildren();
        towerPlacer.Close();
        TowerSpawnManager.HideGhosts();
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
        yield return new WaitForSeconds(0.1f);//Was 1 second
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
    public static void DeselectCurrent()
    {
        if (instance == null) return;
        if (instance._selectedTower != null)
        {
            instance._selectedTower.Deselected();

            instance._selectedTower = null;
        }

        instance._selecting = false;
    }
    public void EndSelection(InputAction.CallbackContext context)
    {
        if (_selectedTower != null && _selectedTower is PlayerControllableTower playerControllableTower)
        {
            _selectedTower.Deselected();
            print($"Is this quick takeover?");
            PlayerStateController.TakeControlOfTower(playerControllableTower);
            _selectedTower = null;
        }
        
        _selecting = false;
    }

    private void OnConfirm()
    {
        if(_selectedTower != null && _selectedTower is PlayerControllableTower playerControllableTower)
        {
            _selectedTower.Deselected();
            PlayerStateController.QuickTakeover(playerControllableTower);
            _selectedTower = null;
            _selecting = false;
        }
        
    }

    #endregion
}