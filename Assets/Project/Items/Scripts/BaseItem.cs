using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class BaseItem : MonoBehaviour
{
    public Inventory _inventory;
    public bool CannotDrop = true;
    public bool dropImmediately = false;
    
    public InputActionReference primaryButton => _inventory.openInventory;
    public InputActionReference stick => _inventory.stick;
    public InputActionReference grip => _inventory.grip;
    public InputActionReference trigger => _inventory.trigger;
    // Start is called before the first frame update

    protected void Awake()
    {
        XRBaseInteractable table = GetComponent<XRBaseInteractable>();
        if (CannotDrop)
            table.selectExited.AddListener(_selectExited);
        else
        {
            table.selectEntered.AddListener(_selectEntered);
        }
    }

    private void OnEnable()
    {
        //if (dropImmediately && _inventory != null)
        //    _inventory.DeselectGO(gameObject);
    }

    void _selectEntered(SelectEnterEventArgs args)
    {
        //_inventory.DeselectGO(gameObject);
    }

    void _selectExited(SelectExitEventArgs args)
    {
        if(_inventory)
            _inventory.SelectGO(gameObject);
    }
}
