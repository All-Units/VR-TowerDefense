using System;
using System.Collections;
using System.Collections.Generic;
using Project.Towers.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class TowerSelector : InventoryItem
{
    public XRControllerTowerPlacer placer;
    private Inventory2 inv;

    [SerializeField] private float openInvTime = 0.5f;
    [SerializeField] private Image fillInvImage;
    [SerializeField] private Image bgInvImage;
    public GameObject inventoryPlatform;
    public GameObject currentTowerIcon;
    public GameObject currentTowerPrice;

    private void Start()
    {
        inventoryPlatform.SetActive(false);
        if (bgInvImage)
            bgInvImage.gameObject.SetActive(false);
    }
    public override void OnPickup(SelectEnterEventArgs args)
    {
        base.OnPickup(args);
        inv = InventoryManager.invByTor(args.interactorObject);
        if (inv == null) { Debug.LogError("Found no inventory!!!"); return; }

        inv.primaryButton.action.started += PrimaryPressed;
        inv.primaryButton.action.canceled += PrimaryReleased;
        placer.inv = inv;
        placer.Pickup();
    }

    public override void OnDrop(SelectExitEventArgs args)
    {
        base.OnDrop(args);
        inv.primaryButton.action.started -= PrimaryPressed;
        inv.primaryButton.action.canceled -= PrimaryReleased;
        placer.Drop();
        CloseInventory();
        
        
    }

    private bool isPrimaryHeld = false;
    void PrimaryPressed(InputAction.CallbackContext obj)
    {
        isPrimaryHeld = true;
        StartCoroutine(_openInv());
    }
    void PrimaryReleased(InputAction.CallbackContext obj)
    {
        isPrimaryHeld = false;
    }

    IEnumerator _openInv()
    {
        float t = 0f;
        bgInvImage.gameObject.SetActive(true);
        //Fill the circle until it's full, or the button is released
        while (t < openInvTime && isPrimaryHeld)
        {
            t += Time.deltaTime;
            float fill = Mathf.Lerp(0, 1, t / openInvTime);
            fillInvImage.fillAmount = fill;
            yield return null;
        }
        bgInvImage.gameObject.SetActive(false);
        //The inventory is to be opened
        if (t >= openInvTime)
        {
            //If the inventory is closed, open it
            if (inventoryPlatform.activeInHierarchy == false)
                OpenInventory();
            else CloseInventory();
        }

        fillInvImage.fillAmount = 0f;

    }

    public void CloseInventory()
    {
        inventoryPlatform.SetActive(false);
        currentTowerIcon.SetActive(true);
        currentTowerPrice.SetActive(true);
    }

    public void OpenInventory()
    {
        inventoryPlatform.SetActive(true);
        currentTowerIcon.SetActive(false);
        currentTowerPrice.SetActive(false);
    }

    private Dictionary<Tower_SO, GameObject> iconsByDTO = new Dictionary<Tower_SO, GameObject>();
    public void SelectTower(Tower_SO dto)
    {
        inventoryPlatform.SetActive(false);
        
        //We've never selected this tower before
        if (iconsByDTO.ContainsKey(dto) == false)
        {
            GameObject icon = Instantiate(dto.iconPrefab, transform);
            currentTowerIcon = icon;
        }
        else
        {
            currentTowerIcon = iconsByDTO[dto];
        }
        TowerSpawnManager.SetTower(dto);
        currentTowerIcon.SetActive(true);
        
    }
}
