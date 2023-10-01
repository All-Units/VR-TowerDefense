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
    public XRGrabInteractable bow;
    public XRGrabInteractable magicStaff;
    public XRGrabInteractable handCannon;
    private XRInteractionManager _manager;

    [SerializeField] private GameObject leftHandParent;
    [SerializeField] private GameObject rightHandParent;

    public static Transform player => instance.playerTransform;

    private void Awake()
    {
        instance = this;
        _manager = FindObjectOfType<XRInteractionManager>();
        invByHand.Add(whichHand.left, leftHand);
        invByHand.Add(whichHand.right, rightHand);
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
        print($"Hid all items");
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

    public static UnityEvent OnItemsHidden = new UnityEvent();

    
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


