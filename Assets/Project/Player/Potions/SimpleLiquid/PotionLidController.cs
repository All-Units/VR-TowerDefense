using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

[RequireComponent(typeof(XRSocketInteractor))]
public class PotionLidController : MonoBehaviour//, IXRHoverFilter, IXRSelectFilter
{
    XRSocketInteractor socket;

    //public bool canProcess => true;

    // Start is called before the first frame update
    void Start()
    {
        socket = GetComponent<XRSocketInteractor>();
        socket.hoverEntered.AddListener(_OnHoverStart);
        socket.hoverExited.AddListener(_OnHoverEnd);
        socket.selectEntered.AddListener(_OnSelectEnter);
        socket.selectExited.AddListener(_OnSelectExit);
        
    }
    void _OnHoverStart(HoverEnterEventArgs a)
    {
        
    }
    void _OnHoverEnd(HoverExitEventArgs a)
    {
        
    }
    bool _hasLid = false;
    void _OnSelectEnter(SelectEnterEventArgs a) 
    {
        
    }
    void _OnSelectExit(SelectExitEventArgs a)
    {
        
    }
    
    bool _IsLid(Transform t)
    {
        return transform.gameObject.tag == "PotionLid";
    }
    /*
    public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
    {
        Debug.Log($"HOVERING on {interactable.transform.gameObject.name}", interactable.transform.gameObject);
        if (_IsLid(interactable.transform) == false) return false;

        return true;
    }

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        Debug.Log($"selecting on {interactable.transform.gameObject.name}", interactable.transform.gameObject);
        if (_IsLid(interactable.transform) == false) return false;
        return true;
        //throw new System.NotImplementedException();
    }*/
}
