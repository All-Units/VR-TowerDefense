using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HapticController : MonoBehaviour
{
    [SerializeField] private float amp, dur;
    
    private List<ActionBasedController> _controllers = new List<ActionBasedController>();
    
    public void SelectEnter(SelectEnterEventArgs args)
    {
            var transformParent = args.interactorObject?.transform?.parent;
            if(transformParent == null) return;
            
            if(transformParent.TryGetComponent(out ActionBasedController actionBasedController))
                _controllers.Add(actionBasedController);
    }   
    
    public void SelectExit(SelectExitEventArgs args)
    {
        var transformParent = args.interactorObject?.transform?.parent;
        if(transformParent == null) return;
            
        if(transformParent.TryGetComponent(out ActionBasedController actionBasedController))
            _controllers.Remove(actionBasedController);
    }

    public void SendHapticPulse(float amplitude, float duration)
    {
        foreach (var controller in _controllers)
        {
            controller.SendHapticImpulse(amplitude, duration);
        }
    }    
    
    public void SendHapticPulse()
    {
        foreach (var controller in _controllers)
        {
            controller.SendHapticImpulse(amp, dur);
        }
    }
}