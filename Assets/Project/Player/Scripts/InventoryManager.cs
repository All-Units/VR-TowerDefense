using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    private Dictionary<whichHand, Inventory> invByHand = new Dictionary<whichHand, Inventory>();

    public Transform playerTransform;
    public XRDirectInteractor playerLeftHand;
    public XRGrabInteractable bow;
    
    private void Awake()
    {
        instance = this;
    }

    public void SpawnBow()
    {
        XRInteractionManager manager = FindObjectOfType<XRInteractionManager>();
        manager.SelectEnter((IXRSelectInteractor)playerLeftHand, bow);
    }

    public void SpawnItemNearPlayer(GameObject item)
    {
        var go = Instantiate(item, playerTransform.position + playerTransform.forward, playerTransform.rotation);
    }
}


