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
    private Dictionary<WhichHand, Inventory2> invByHand = new Dictionary<WhichHand, Inventory2>();

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

    bool _isLeftHanded = false;

    private void Awake()
    {
        instance = this;
        _manager = FindObjectOfType<XRInteractionManager>();
        invByHand.Add(WhichHand.left, leftHand);
        invByHand.Add(WhichHand.right, rightHand);
        cannonAmmoPouch.gameObject.SetActive(false);

        string isLeft = PlayerPrefs.GetString("_isPlayerLeftHanded", "false");
        _isLeftHanded = false;
        if (isLeft.Equals("true"))
            _isLeftHanded = true;
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

    PlayerItem_SO _lastHeldItem;
    public void GivePlayerItem(PlayerItem_SO playerItemType)
    {
        _lastHeldItem = playerItemType;
        ReleaseAllItems();
        HideAllItems();

        if (_playerItems.ContainsKey(playerItemType) == false) 
        {
            var newWeapon = Instantiate(playerItemType.itemGo,playerItemsTransformRoot);
            _playerItems.Add(playerItemType, newWeapon.GetComponent<XRGrabInteractable>());
            ResetOnDrop drop = newWeapon.GetComponent<ResetOnDrop>();
            if (drop != null)
            {
                drop.playerItem = playerItemType;
            }

            //Add listener for on first entered on XR object
            var xr = _playerItems[playerItemType];
            xr.firstSelectEntered.AddListener(_OnPickupItem);
        }

        var item = _playerItems[playerItemType];


        //Default to holding items in right hand
        var primaryHand = playerRightHand;
        var secondaryHand = playerLeftHand;
        bool offhand = playerItemType.holdInOffHand;

        //Left handed players flip their primary hand
        if (_isLeftHanded) 
            offhand = !offhand;
        
        if (offhand)
        {
            primaryHand = playerLeftHand;
            secondaryHand = playerRightHand;
        }
        primaryHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
        secondaryHand.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;
        _manager.SelectEnter((IXRSelectInteractor)primaryHand, item);

        
        /*if (playerItemType.holdInOffHand)
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
        }*/
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

        foreach (var itemsValue in _playerItems.Values)
        {
            var selectInteractors = itemsValue.interactorsSelecting;
            for (var i = 1; i < selectInteractors.Count; i++)
            {
                _manager.SelectExit(selectInteractors[i], itemsValue);
            }
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

    #region HelperFunctions
    /// <summary>
    /// Checks that the correct hand is holding the player item
    /// </summary>
    /// <param name="a"></param>
    void _OnPickupItem(SelectEnterEventArgs a)
    {
        XRControllerTowerController.DeselectCurrent();

        //If the object is held by the right hand
        bool isRight = a.interactorObject.transform.gameObject == playerRightHand.gameObject;
        //If the object is held by the left hand
        bool isLeft = a.interactorObject.transform.gameObject == playerLeftHand.gameObject;

        //Whether the player is supposed to be holding this item in their off hand
        bool offhand = _lastHeldItem.holdInOffHand;

        //If we're left handed, off hand means right
        if (_isLeftHanded) offhand = !offhand;

        //If right handed, and weapon is not an off-hand weapon, we should be right
            //If left handed, our right is our off-hand
        bool shouldBeRight = (offhand == false);

        //Checks if the right hand is empty, and supposed to be empty, and that those match
        bool isCorrect = isRight == shouldBeRight;

        
        //Do nothing if in correct hand
        if (isCorrect) return;

        var primary = playerRightHand;
        var secondary = playerLeftHand;
        if (isLeft)
        {
            primary = playerLeftHand;
            secondary = playerRightHand;
        }
        //Make sure the correct hand is sticky
        primary.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
        secondary.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;
        _isLeftHanded = !_isLeftHanded;
        
        string leftString = _isLeftHanded ? "true" : "false";
        //Serialize
        PlayerPrefs.SetString("_isPlayerLeftHanded", leftString);

    }
    

    public bool RightHandFull() => playerRightHand.interactablesSelected.Any();
    public bool LeftHandFull() => playerLeftHand.interactablesSelected.Any();
    /// <summary>
    /// Gets all objects currently held in both hands
    /// </summary>
    /// <returns></returns>
    public static List<IXRSelectInteractable> CurrentlyHeldObjects()
    {
        List<IXRSelectInteractable> held = new List<IXRSelectInteractable>();
        held.AddRange(instance.playerLeftHand.interactablesSelected);
        held.AddRange(instance.playerRightHand.interactablesSelected);

        return held;
    }

    public static bool IsObjLeftHand(GameObject go)
    {
        if (instance == null) return false;
        return go == instance.playerLeftHand.gameObject;
    }
    public static bool IsObjRightHand(GameObject go)
    {
        if (instance == null) return false;
        return go == instance.playerRightHand.gameObject;
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
public enum WhichHand
{
    left,
    right
}