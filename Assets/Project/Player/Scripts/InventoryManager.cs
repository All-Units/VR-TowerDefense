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
    public Transform playerCameraTransform;
    public Inventory2 leftHand;
    public Inventory2 rightHand;
    public XRDirectInteractor playerLeftHand;
    public XRDirectInteractor playerRightHand;
    [SerializeField] private GameObject leftHandParent;
    [SerializeField] private GameObject rightHandParent;

    #region Player Item Variables

    [Header("Player Tower Items")] 
    private Dictionary<PlayerItem_SO, XRGrabInteractable> _playerItems = new();

    [SerializeField] private Transform playerItemsTransformRoot;
    
    [Header("Player Tower Item Extras")]
    public XRInstantiateGrabbableObject quiver;
    public XRInstantiateGrabbableObject cannonAmmoPouch;

    [SerializeField] private FireballGlovesController leftFireballGlovesController, rightFireballGlovesController;
    [SerializeField] private FireLanceGlovesController leftFlameLanceGlovesController, rightFlameLanceGlovesController;

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

    private void SpawnBow(XRGrabInteractable item)
    {
        playerLeftHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
        playerRightHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;

        _manager.SelectEnter((IXRSelectInteractor)playerLeftHand, item);
    }

    private void SpawnMagicStaff(XRGrabInteractable item)
    {
        playerRightHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
        _manager.SelectEnter((IXRSelectInteractor)playerRightHand, item);
    }

    private void SpawnHandCannon(XRGrabInteractable item)
    {
        playerRightHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
        _manager.SelectEnter((IXRSelectInteractor)playerRightHand, item);
    }

    #endregion

    #region Item Lifecyle

    public void GivePlayerItem(PlayerItem_SO playerItemType)
    {
        ReleaseAllItems();
        HideAllItems();

        if (_playerItems.ContainsKey(playerItemType) == false) 
        {
            var newWeapon = Instantiate(playerItemType.itemGo,playerItemsTransformRoot);
            _playerItems.Add(playerItemType, newWeapon.GetComponent<XRGrabInteractable>());
        }

        var item = _playerItems[playerItemType];
        
        if (playerItemType.holdInOffHand)
        {
            playerLeftHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
            playerRightHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;

            _manager.SelectEnter((IXRSelectInteractor)playerLeftHand, item);
        }
        else
        {
            playerLeftHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;
            playerRightHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;

            _manager.SelectEnter((IXRSelectInteractor)playerRightHand, item);
        }
    }

    public void GivePlayerPower(PlayerPower playerPower)
    {
        switch (playerPower.type)
        {
            case PlayerPowerType.FIREBALLS:
                leftFireballGlovesController.gameObject.SetActive(true);
                rightFireballGlovesController.gameObject.SetActive(true);
                leftFireballGlovesController.SetThrowable(playerPower.throwable);
                rightFireballGlovesController.SetThrowable(playerPower.throwable);
                break;
            case PlayerPowerType.FLAMELANCE:
                leftFlameLanceGlovesController.gameObject.SetActive(true);
                rightFlameLanceGlovesController.gameObject.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        
        playerLeftHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;
        playerRightHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;
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
        foreach (Transform t in instance._playerItems.Values.Select(i => i.transform))
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

    public void ActivateItemExtra(PlayerItem_SO data)
    {
        switch (data.ammoPouch)
        {
            case PlayerItem_SO.ItemAmmoPouch.NONE:
                break;
            case PlayerItem_SO.ItemAmmoPouch.ARROW_QUIVER:
                quiver.gameObject.SetActive(true);
                quiver.SetAmmoPrefab(data.ammo.gameObject);
                break;
            case PlayerItem_SO.ItemAmmoPouch.BOMB_SATCHEL:
                cannonAmmoPouch.gameObject.SetActive(true);
                cannonAmmoPouch.SetAmmoPrefab(data.ammo.gameObject);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(data.ammoPouch), data.ammoPouch, null);
        }
    }

    public void DeactivateItemExtra(PlayerItem_SO data)
    {
        switch (data.ammoPouch)
        {
            case PlayerItem_SO.ItemAmmoPouch.NONE:
                break;
            case PlayerItem_SO.ItemAmmoPouch.ARROW_QUIVER:
                quiver.gameObject.SetActive(false);
                break;
            case PlayerItem_SO.ItemAmmoPouch.BOMB_SATCHEL:
                cannonAmmoPouch.gameObject.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(data.ammoPouch), data.ammoPouch, null);
        }
    }

    private void DeactivateAllItemExtras()
    {
        quiver.gameObject.SetActive(false);
        cannonAmmoPouch.gameObject.SetActive(false);
        
        leftFireballGlovesController.gameObject.SetActive(false);
        rightFireballGlovesController.gameObject.SetActive(false);
        leftFlameLanceGlovesController.gameObject.SetActive(false);
        rightFlameLanceGlovesController.gameObject.SetActive(false);
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