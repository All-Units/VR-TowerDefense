using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class Inventory2 : MonoBehaviour
{
    public HashSet<IXRSelectInteractor> tors = new HashSet<IXRSelectInteractor>();
    public InputActionReference primaryButton;
    public InputActionReference trigger;
    private void Awake()
    {
        tors = GetComponentsInChildren<IXRSelectInteractor>().ToHashSet();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
