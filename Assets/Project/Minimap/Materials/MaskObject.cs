using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskObject : MonoBehaviour
{
    public MeshRenderer mr;
    // Start is called before the first frame update
    public void Awake()
    {
        return;
        if (mr == null)
            mr = GetComponent<MeshRenderer>();
        foreach (Material mat in mr.materials)
            mat.renderQueue = 3002;
        //mr.enabled = false;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
