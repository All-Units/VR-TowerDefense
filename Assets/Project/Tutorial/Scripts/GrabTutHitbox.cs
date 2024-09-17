using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabTutHitbox : MonoBehaviour
{
    public GrabTutorial tutorial;
    HashSet<Collider> colliders = new HashSet<Collider>();
    private void OnTriggerEnter(Collider other)
    {
        other.transform.parent = transform;
        
        if (colliders.Contains(other)) return;
        XRGrabInteractable grab = other.GetComponent<XRGrabInteractable>();
        insideBox.Add(grab);
        //If there is nothing holding on to the ammo, do the logic immediately
        if (grab.interactorsSelecting.Count == 0)
        {
            colliders.Add(other);
            tutorial.OnCannonballEnterBox();
        }
        //We're still held, wait until dropped to add to count
        else
        {
            grab.lastSelectExited.RemoveAllListeners();
            grab.lastSelectExited.AddListener(_AmmoReleased);
        }
        
    }
    HashSet<XRGrabInteractable> insideBox = new HashSet<XRGrabInteractable>();
    void _AmmoReleased(SelectExitEventArgs a)
    {
        XRGrabInteractable grab = a.interactableObject.transform.GetComponent<XRGrabInteractable>();
        if (insideBox.Contains(grab))
        {
            Collider other = grab.GetComponent<Collider>();
            colliders.Add(other);
            tutorial.OnCannonballEnterBox();
            grab.lastSelectExited.RemoveAllListeners();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        XRGrabInteractable grab = other.GetComponent<XRGrabInteractable>();
        insideBox.Remove(grab);
    }
}
