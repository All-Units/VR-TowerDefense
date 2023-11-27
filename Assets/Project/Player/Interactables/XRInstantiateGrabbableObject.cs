using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Read as ammo pouch. Basically on select, spawn and grab the assigned item.
/// </summary>
public class XRInstantiateGrabbableObject : XRBaseInteractable
{
    [SerializeField]
    private GameObject grabbableObject;

    public void SetAmmoPrefab(GameObject pref)
    {
        grabbableObject = pref;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Instantiate object
        GameObject newObject = Instantiate(grabbableObject);
        
        // Get grab interactable from prefab
        XRGrabInteractable objectInteractable = newObject.GetComponent<XRGrabInteractable>();
        
        // Select object into same interactor
        interactionManager.SelectEnter(args.interactorObject, objectInteractable);
        
        base.OnSelectEntered(args);
    }
}