using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public enum whichHand
{
    left,
    right
}
public class Inventory : MonoBehaviour
{
    #region PublicVariables
    public whichHand hand = whichHand.left;
    [Header("Input References")]
    [SerializeField] public InputActionReference openInventory;
    [SerializeField] public InputActionReference stick;
    [SerializeField] public InputActionReference grip;
    [SerializeField] public InputActionReference trigger;
    
    [Header("GameObject references")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform arrowParent;
    [SerializeField] private Transform iconsParent;
    [SerializeField] private Transform dividingLinesParent;
    [SerializeField] private Transform itemsParent;
    [SerializeField] private MovementLocker _locker;
    [SerializeField] private float angleOffset = 90f;
    [SerializeField] private List<Item_SO> Items = new List<Item_SO>();
    public GameObject interactorGO;
    public XRInteractionManager interactionManager;
    #endregion


    #region UnityEvents
    // Start is called before the first frame update
    void Start()
    {
        FillWheel();
        inventoryPanel.SetActive(false);

    }

    private void OnEnable()
    {
        openInventory.action.performed += _onOpenInventory;
    }

    private void OnDisable()
    {
        openInventory.action.performed -= _onOpenInventory;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    #endregion


    #region ControlFunctions
    void _onOpenInventory(InputAction.CallbackContext obj)
    {
        if (IsOpen)
            _onClose();
        else
        {
            _onOpen();
        }
        
    }
    
    void _onOpen()
    {
        _selectCurrentAngle();
        inventoryPanel.SetActive(true); 
        PlaceLock(gameObject);
        stick.action.performed += PointArrow;
    }

    

    void _onClose()
    {
        inventoryPanel.SetActive(false);
        RemoveLock(gameObject);
        stick.action.performed -= PointArrow;
    }

    private int current_icon_i;
    void PointArrow(InputAction.CallbackContext obj)
    {
        Vector2 dir = obj.ReadValue<Vector2>().normalized;
        float degrees = Mathf.Atan2(dir.y, dir.x) * (180f / Mathf.PI);
        degrees += angleOffset;
        Vector3 euler = arrowParent.localEulerAngles;
        euler.z = degrees;
        arrowParent.localEulerAngles = euler;
        _selectCurrentAngle();
    }
    #endregion

    #region HelperFunctions

    private void _selectCurrentAngle(float angle = float.NegativeInfinity)
    {
        if (angle < -10000f)
            angle = arrowParent.localEulerAngles.z;
        int i = (int)((360f - angle) / arc);
        if (i != current_icon_i)
        {
            int old = current_icon_i;
            current_icon_i = i;
            SelectItem(i, old);
        }
    }
    public void PlaceLock(GameObject go)
    {
        _locker.PlaceMovementLock(go);
    }

    public void RemoveLock(GameObject go)
    {
        _locker.RemoveMovementLock(go);
    }
    public void SelectItem(int i, int old)
    {
        //Ignore i's out of bounds (likely from float -> int conversion errors)
        if (i >= Items.Count || i < 0) return;
        old = Mathf.Clamp(old, 0, spawnedItems.Count - 1);
        _itemIcons[i].OnSelect();
        GameObject active = spawnedItems[i];
        active.SetActive(true);
        print($"Trying to turn off {old}");
        var off = spawnedItems[old];
        off.SetActive(false);
        
        SelectGO(active);
        
        
    }

    private float arc => (360f / (float)Items.Count);
    private Dictionary<Item_SO, float> anglesByItem = new Dictionary<Item_SO, float>();
    private List<ItemIcon> _itemIcons = new List<ItemIcon>();
    private List<GameObject> spawnedItems = new List<GameObject>();
    void FillWheel()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            Item_SO item = Items[i];
            GameObject icon = Instantiate(item.itemIconPrefab, iconsParent);
            
            GameObject spawnedItem = Instantiate(item.itemPrefab.gameObject);
            BaseItem baseItem = spawnedItem.GetComponent<BaseItem>();
            baseItem._inventory = this;
            baseItem.enabled = true;
            
            spawnedItems.Add(spawnedItem);
            spawnedItem.SetActive(false);

            
                
            float degrees = arc * i + (arc / 2f);
            anglesByItem.Add(item, degrees);
            icon.transform.localEulerAngles = new Vector3(0f, 0f, -1f * degrees);
            ItemIcon itemIcon = icon.GetComponent<ItemIcon>();
            itemIcon.iconSprite.transform.localEulerAngles = new Vector3(0f, 0f, degrees);
            _itemIcons.Add(itemIcon);
            if (i != 0)
            {
                GameObject line = Instantiate(dividingLinesParent.GetChild(0).gameObject, dividingLinesParent);
                line.transform.localEulerAngles = new Vector3(0f, 0f, arc * i);
            }
        }
    }
    public bool IsOpen => inventoryPanel.activeInHierarchy;

    public void SelectGO(GameObject go)
    {
        var table = go.GetComponentInChildren<IXRSelectInteractable>();
        IXRSelectInteractor tor = interactorGO.GetComponentInChildren<IXRSelectInteractor>();
        print($"Manually selecting {go.name}. Currently selected is {tor.firstInteractableSelected}");
        var current = tor.firstInteractableSelected;
        if (current != null)
            interactionManager.SelectExit(tor, current);
        BaseItem item = go.GetComponentInChildren<BaseItem>();
        if (item.CannotDrop)
            interactionManager.SelectEnter(tor, table);
        
    }

    public void DeselectGO(GameObject go)
    {
        IXRSelectInteractor tor = interactorGO.GetComponentInChildren<IXRSelectInteractor>();
        if (tor.firstInteractableSelected != null)
        {
            print($"We had an interactable, so stopping that");
            interactionManager.SelectExit(tor, tor.firstInteractableSelected);
        }
        //StartCoroutine(_cycleDrop(go));
        return;
        
    }

    IEnumerator _cycleDrop(GameObject go)
    {
        yield return null;
        IXRSelectInteractable table = go.GetComponentInChildren<IXRSelectInteractable>();
        IXRSelectInteractor tor = interactorGO.GetComponentInChildren<IXRSelectInteractor>();
        interactionManager.SelectEnter(tor, table);
        yield return null;
        interactionManager.SelectExit(tor, table);
        print($"Manually dropped {go.name}");
    }
    
    #endregion
    
    

    
    #region Legacy
    /// <summary>
    /// Need to massively refactor
    /// </summary>
    

    
    
    
    
    
    #endregion
}
