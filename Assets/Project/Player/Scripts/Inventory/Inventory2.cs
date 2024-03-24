using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

public class Inventory2 : MonoBehaviour
{
    public whichHand whichHand = whichHand.left;
    public HashSet<IXRSelectInteractor> tors = new HashSet<IXRSelectInteractor>();
    public InputActionReference primaryButton;
    public InputActionReference SecondaryButton;
    public InputActionReference trigger;
    
    private void Awake()
    {
        tors = GetComponentsInChildren<IXRSelectInteractor>().ToHashSet();
        if (whichHand == whichHand.left)
        {
            SecondaryButton.action.started += SecondaryPressed;
            SecondaryButton.action.canceled += SecondaryReleased;
        }
    }

    private void OnDestroy()
    {
        if (whichHand == whichHand.left)
        {
            SecondaryButton.action.started -= SecondaryPressed;
            SecondaryButton.action.canceled -= SecondaryReleased;
        }
        
    }

    private void SecondaryPressed(InputAction.CallbackContext obj)
    {
        if (BasicPauseMenu.instance == null)
        {
            //Debug.LogError("BasicPauseMenu singleton is null");
            return;
        }
        
        BasicPauseMenu.instance.IsSecondaryHeld = true;
        BasicPauseMenu.StartPauseFill();
    }
    private void SecondaryReleased(InputAction.CallbackContext obj)
    {
        if (BasicPauseMenu.instance == null)
        {
            //Debug.LogError("BasicPauseMenu singleton is null");
            return;
        }

        BasicPauseMenu.instance.IsSecondaryHeld = false;
    }

    

   
    
}
