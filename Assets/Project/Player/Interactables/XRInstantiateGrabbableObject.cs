using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRInstantiateGrabbableObject : XRBaseInteractable
{
    [SerializeField]
    private GameObject grabbableObject;

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