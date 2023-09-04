using System.Collections.Generic;
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
    public XRGrabInteractable bow;
    
    private void Awake()
    {
        instance = this;
        invByHand.Add(whichHand.left, leftHand);
        invByHand.Add(whichHand.right, rightHand);
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


