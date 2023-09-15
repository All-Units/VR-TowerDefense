using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MinimapItem : InventoryItem
{
    [SerializeField] private GameObject minimapParent;
    protected override void Awake()
    {
        base.Awake();
        grabber.activated.AddListener(ActivatePressed);
        Minimap.SetActive(false);
    }
    void ActivatePressed(ActivateEventArgs args)
    {
        Minimap.SetActive(!Minimap.IsActive());
        minimapParent.transform.position = transform.position;
    }
}
