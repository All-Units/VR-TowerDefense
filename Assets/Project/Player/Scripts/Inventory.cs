using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public enum whichHand
{
    left,
    right
}
public class Inventory : MonoBehaviour
{
    public whichHand hand = whichHand.left;
    [SerializeField] public InputActionReference openInventory;
    [SerializeField] public InputActionReference stick;
    [SerializeField] public InputActionReference grip;
    [SerializeField] public InputActionReference trigger;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform arrowParent;
    [SerializeField] private Transform iconsParent;
    [SerializeField] private Transform dividingLinesParent;
    [SerializeField] private Transform itemsParent;
    [SerializeField] private float angleOffset = 90f;
    [SerializeField] private List<Item_SO> Items = new List<Item_SO>();


    /// <summary>
    /// Can enable / disable movement
    /// </summary>
    public ActionBasedControllerManager acbm;

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

    public bool IsOpen => inventoryPanel.activeInHierarchy;
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
        inventoryPanel.SetActive(true);
        PlaceMovementLock(gameObject);
        stick.action.performed += PointArrow;
    }

    void _onClose()
    {
        inventoryPanel.SetActive(false);
        RemoveMovementLock(gameObject);
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
        float deg = arrowParent.localEulerAngles.z;
        int i = (int)((360f - deg) / arc);
        if (i != current_icon_i)
        {
            int old = current_icon_i;
            current_icon_i = i;
            SelectItem(i, old);
        }
    }
    public void SelectItem(int i, int old)
    {
        //Ignore i's out of bounds (likely from float -> int conversion errors)
        if (i >= Items.Count || i < 0) return;
        old = Mathf.Clamp(old, 0, itemsParent.childCount);
        _itemIcons[i].OnSelect();
        itemsParent.GetChild(i).gameObject.SetActive(true);
        itemsParent.GetChild(old).gameObject.SetActive(false);
    }

    private float arc => (360f / (float)Items.Count);
    private Dictionary<Item_SO, float> anglesByItem = new Dictionary<Item_SO, float>();
    private List<ItemIcon> _itemIcons = new List<ItemIcon>();
    void FillWheel()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            Item_SO item = Items[i];
            GameObject icon = Instantiate(item.itemIconPrefab, iconsParent);

            GameObject spawnedItem = Instantiate(item.itemPrefab.gameObject, itemsParent);
            BaseItem baseItem = spawnedItem.GetComponent<BaseItem>();
            baseItem._inventory = this;
            
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
    
    /// <summary>
    /// Places a movement lock, depending on the hand the item resides in
    /// </summary>
    public virtual void PlaceMovementLock(GameObject caller)
    {
        if (hand == whichHand.left)
            PlaceMovementLockLeft();
        else 
            PlaceMovementLockRight(caller);
    }
    /// <summary>
    /// Removes the correct movement lock from the correct hand
    /// </summary>
    public virtual void RemoveMovementLock(GameObject caller)
    {
        if (hand == whichHand.left)
            RemoveMovementLockLeft();
        else RemoveMovementLockRight(caller);
    }

    public virtual void PlaceMovementLockLeft()
    {
        DynamicMoveProvider.AddMovementLock();
    }

    private HashSet<GameObject> lockedObjs = new HashSet<GameObject>();
    public virtual void PlaceMovementLockRight(GameObject caller)
    {
        //If the object calling the lock is not already locking movement
        if (lockedObjs.Contains(caller) == false)
        {
            acbm.FreezeMovement();
            lockedObjs.Add(caller); 
            //print($"Freezing movement");
        }
        //Else do nothing
    }

    public virtual void RemoveMovementLockLeft()
    {
        DynamicMoveProvider.RemoveMovementLock();
    }

    public virtual void RemoveMovementLockRight(GameObject caller)
    {
        //Only do anything if the obj is locked
        if (lockedObjs.Contains(caller))
        {
            acbm.EnableLocomotionActions();
            lockedObjs.Remove(caller);
        }
    }
}
