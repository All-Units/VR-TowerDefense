using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereDetector : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Inventory") == false) return;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Inventory") == false) return;
    }
}
