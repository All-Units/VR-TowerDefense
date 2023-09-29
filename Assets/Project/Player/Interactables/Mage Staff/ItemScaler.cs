using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ItemScaler : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable table;
    private Vector3 startScale = Vector3.negativeInfinity;

    private void Awake()
    {
        startScale = transform.localScale;
        table.selectEntered.AddListener(OnSelect);
    }

    public void OnSelect(SelectEnterEventArgs args)
    {
        transform.parent = null;
        ResetScale("OnSelect");
    }

    private void OnEnable()
    {
        ResetScale("OnEnable");
    }

    public void ResetScale(string caller = "")
    {
        if (startScale.y < -10000) return;
        
        
        Transform t = transform;
        t.parent = null;
        Vector3 scale = t.localScale;
        if (Vector3.Distance(scale, startScale) > 0.001f)
        {
            t.localScale = startScale;
            string s = (caller == "") ? "" : $". Called by: {caller}";
            //Debug.Log($"{name} was the wrong scale. Was {scale}. Local scale is {t.localScale}. Setting to start val of {startScale}{s}", gameObject);
        }
    }
}
