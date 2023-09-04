using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    private Dictionary<whichHand, Inventory> invByHand = new Dictionary<whichHand, Inventory>();

    public Transform playerTransform;
    public XRDirectInteractor playerLeftHand;
    public XRDirectInteractor playerRightHand;
    public XRGrabInteractable bow;
    public XRGrabInteractable magicStaff;
    public XRGrabInteractable handCannon;
    private XRInteractionManager _manager;

    private void Awake()
    {
        instance = this;
        _manager = FindObjectOfType<XRInteractionManager>();
    }
    

    public void SpawnBow()
    {
        playerLeftHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
        playerRightHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;

        _manager.SelectEnter((IXRSelectInteractor)playerLeftHand, bow);
    }

    public void SpawnMagicStaff()
    {
        playerRightHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
        _manager.SelectEnter((IXRSelectInteractor)playerRightHand, magicStaff);
    }

    public void SpawnHandCannon()
    {
        playerRightHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
        _manager.SelectEnter((IXRSelectInteractor)playerRightHand, handCannon);
    }

    public void SpawnItemNearPlayer(GameObject item)
    {
        var go = Instantiate(item, playerTransform.position + playerTransform.forward, playerTransform.rotation);
    }

    public void GivePlayerItem(PlayerItemType playerItemType)
    {
        ReleaseAllItems();

        switch (playerItemType)
        {
            case PlayerItemType.Bow:
                SpawnBow();
                break;
            case PlayerItemType.Staff:
                SpawnMagicStaff();
                break;
            case PlayerItemType.Cannon:
                SpawnHandCannon();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(playerItemType), playerItemType, null);
        }
    }

    public void ReleaseAllItems()
    {
        ReleaseAllSelected(playerRightHand);
        ReleaseAllSelected(playerLeftHand);
    }

    private void ReleaseAllSelected(XRBaseInteractor interactor)
    {
        var selectedInteractables = interactor.interactablesSelected.ToArray();

        foreach (var selectedInteractable in selectedInteractables)
        {
            _manager.SelectExit(interactor, selectedInteractable);
        }
    }

    public bool RightHandFull() => playerRightHand.interactablesSelected.Any();
    public bool LeftHandFull() => playerLeftHand.interactablesSelected.Any();
}


