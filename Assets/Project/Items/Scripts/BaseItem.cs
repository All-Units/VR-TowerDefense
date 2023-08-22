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
    public InputActionReference primaryButton => _inventory.openInventory;
    public InputActionReference stick => _inventory.stick;
    public InputActionReference grip => _inventory.grip;
    public InputActionReference trigger => _inventory.trigger;
    // Start is called before the first frame update

    protected void Awake()
    {
        XRBaseInteractable interactor = GetComponent<XRBaseInteractable>();
        interactor.selectExited.AddListener(_selectExited);
    }

    void _selectExited(SelectExitEventArgs args)
    {
        if (CannotDrop == false) return;
        _inventory.SelectGO(gameObject);
    }
}
