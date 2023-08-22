using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class InventoryManager : MonoBehaviour
{
    
    public static InventoryManager instance;
    private Dictionary<whichHand, Inventory> invByHand = new Dictionary<whichHand, Inventory>();
    private void Awake()
    {
        instance = this;
    }

   
}
