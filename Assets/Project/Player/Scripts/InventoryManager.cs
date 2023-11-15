using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
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
    [SerializeField] private GameObject leftHandParent;
    [SerializeField] private GameObject rightHandParent;

    #region Player Item Variables

    [Header("Player Tower Items")]
    public XRGrabInteractable bow;
    public XRGrabInteractable magicStaff;
    public XRGrabInteractable handCannon;
    
    private static List<Transform> itemTransforms {
        get
        {
            //Init item list if it doesn't exist
            if (_itemTransforms.Count == 0)
            {
                _itemTransforms.Add(instance.bow.transform);
                _itemTransforms.Add(instance.magicStaff.transform);
                _itemTransforms.Add(instance.handCannon.transform);
            }
            return _itemTransforms;
        }
    }
    private static List<Transform> _itemTransforms = new List<Transform>();
    
    [Header("Player Tower Item Extras")]
    public XRInstantiateGrabbableObject quiver;

    #endregion

    
    private XRInteractionManager _manager;

    // Todo: Should be Refactored out. No need to couple the Inventory manager and the minimap 
    public static Transform player => instance.playerTransform;
    
    public static UnityEvent OnItemsHidden = new UnityEvent();

    #region Unity Interface

    private void Awake()
    {
        instance = this;
        _manager = FindObjectOfType<XRInteractionManager>();
        invByHand.Add(whichHand.left, leftHand);
        invByHand.Add(whichHand.right, rightHand);
    }

    #endregion


    #region Item Initialization

    private void SpawnBow()
    {
        playerLeftHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
        playerRightHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;

        _manager.SelectEnter((IXRSelectInteractor)playerLeftHand, bow);
    }

    private void SpawnMagicStaff()
    {
        playerRightHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
        _manager.SelectEnter((IXRSelectInteractor)playerRightHand, magicStaff);
    }

    private void SpawnHandCannon()
    {
        playerRightHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
        _manager.SelectEnter((IXRSelectInteractor)playerRightHand, handCannon);
    }

    #endregion

    #region Item Lifecyle

    public void GivePlayerItem(PlayerItemType playerItemType)
    {
        ReleaseAllItems();
        HideAllItems();
        
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
        var tors = new List<XRBaseInteractor>();
        if (tors.Count == 0)
        {
            tors = tors.Concat(leftHandParent.GetComponentsInChildren<XRBaseInteractor>()).ToList();
            tors = tors.Concat(rightHandParent.GetComponentsInChildren<XRBaseInteractor>()).ToList();
        }

        foreach (var tor in tors)
        {
            ReleaseAllSelected(tor);
        }
        
        DeactivateAllItemExtras();
    }


    /// <summary>
    /// Moves all items out of the map
    /// </summary>
    public static void HideAllItems()
    {
        foreach (Transform t in itemTransforms)
        {
            //Move down 100
            t.Translate(Vector3.down * 100f);
            Rigidbody rb = t.GetComponent<Rigidbody>();
            //Freeze
            if (rb)
            {
                instance.StartCoroutine(_ResetRigidbody(rb));
            }
        }
        OnItemsHidden.Invoke();
    }

    static IEnumerator _ResetRigidbody(Rigidbody rb)
    {
        rb.velocity = Vector3.zero;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        yield return new WaitForSeconds(0.1f);
        rb.constraints = RigidbodyConstraints.None;
        rb.velocity = Vector3.zero;
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

    public void ActivateItemExtra(PlayerItem_SO.ItemAmmoPouch ammoPouch)
    {
        switch (ammoPouch)
        {
            case PlayerItem_SO.ItemAmmoPouch.NONE:
                break;
            case PlayerItem_SO.ItemAmmoPouch.ARROW_QUIVER:
                quiver.gameObject.SetActive(true);
                break;
            case PlayerItem_SO.ItemAmmoPouch.BOMB_SATCHEL:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(ammoPouch), ammoPouch, null);
        }
    }

    public void DeactivateItemExtra(PlayerItem_SO.ItemAmmoPouch ammoPouch)
    {
        switch (ammoPouch)
        {
            case PlayerItem_SO.ItemAmmoPouch.NONE:
                break;
            case PlayerItem_SO.ItemAmmoPouch.ARROW_QUIVER:
                quiver.gameObject.SetActive(false);
                break;
            case PlayerItem_SO.ItemAmmoPouch.BOMB_SATCHEL:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(ammoPouch), ammoPouch, null);
        }
    }

    public void DeactivateAllItemExtras()
    {
        foreach (PlayerItem_SO.ItemAmmoPouch value in Enum.GetValues(typeof(PlayerItem_SO.ItemAmmoPouch)))
        {
            DeactivateItemExtra(value);
        }
    }

    #endregion

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