using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    private Dictionary<whichHand, Inventory2> invByHand = new Dictionary<whichHand, Inventory2>();

    public Transform playerTransform;
    public Inventory2 leftHand;
    public Inventory2 rightHand;
    public XRDirectInteractor playerLeftHand;
    public XRDirectInteractor playerRightHand;
    public XRGrabInteractable bow;
    public XRGrabInteractable magicStaff;
    public XRGrabInteractable handCannon;
    private XRInteractionManager _manager;

    [SerializeField] private GameObject leftHandParent;
    [SerializeField] private GameObject rightHandParent;

    [SerializeField] private float _inventoryYOffset = -.3f;
    [SerializeField] private Transform playerCamera;
    public static float InventoryY => instance.playerCamera.position.y + instance._inventoryYOffset;
    

    private void Awake()
    {
        instance = this;
        _manager = FindObjectOfType<XRInteractionManager>();
        invByHand.Add(whichHand.left, leftHand);
        invByHand.Add(whichHand.right, rightHand);
    }
    

    public void SpawnBow()
    {
        resetItem(bow);
        playerLeftHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
        playerRightHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;

        _manager.SelectEnter((IXRSelectInteractor)playerLeftHand, bow);
    }

    public void SpawnMagicStaff()
    {
        resetItem(magicStaff);
        playerRightHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
        _manager.SelectEnter((IXRSelectInteractor)playerRightHand, magicStaff);
    }

    public void SpawnHandCannon()
    {
        resetItem(handCannon);
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

    void resetItem(XRGrabInteractable go)
    {
        ItemScaler scaler = go.GetComponent<ItemScaler>();
        scaler.ResetScale();
    }

    private List<XRBaseInteractor> _tors = new List<XRBaseInteractor>();
    public void ReleaseAllItems()
    {
        //ReleaseAllSelected(playerRightHand);
        //ReleaseAllSelected(playerLeftHand);
        if (_tors.Count == 0)
        {
            _tors = _tors.Concat(leftHandParent.GetComponentsInChildren<XRBaseInteractor>()).ToList();
            _tors = _tors.Concat(rightHandParent.GetComponentsInChildren<XRBaseInteractor>()).ToList();
        }

        foreach (var tor in _tors)
        {
            ReleaseAllSelected(tor);
        }
    }

    
    private void ReleaseAllSelected(XRBaseInteractor interactor)
    {
        
        var selectedInteractables = interactor.interactablesSelected.ToArray();

        bool dropped = false;
        foreach (var selectedInteractable in selectedInteractables)
        {
            _manager.SelectExit(interactor, selectedInteractable);
        }
        
    }

    public bool RightHandFull() => playerRightHand.interactablesSelected.Any();
    public bool LeftHandFull() => playerLeftHand.interactablesSelected.Any();
    public static Inventory2 invByTor(IXRSelectInteractor tor)
    {
        foreach (var inv in instance.invByHand.Values)
        {
            if (inv.tors.Contains(tor))
                return inv;
        }

        return null;
    }
}


