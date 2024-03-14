using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Ammo : MonoBehaviour
{
    XRGrabInteractable grab;
    private void Start()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectExited.AddListener(_DestroyAfter);
        grab.selectEntered.AddListener(_StopDestroy);
    }
    IEnumerator _currentDestroyer = null;
    void _DestroyAfter(SelectExitEventArgs args)
    {
        _currentDestroyer = Utilities._DestroyAfter(gameObject, 3f);
        StartCoroutine(_currentDestroyer);
        //gameObject.DestroyAfter(3f);
    }
    void _StopDestroy(SelectEnterEventArgs args)
    {
        if (_currentDestroyer != null)
        {
            StopCoroutine(_currentDestroyer);
            _currentDestroyer = null;
        }
    }
    
}