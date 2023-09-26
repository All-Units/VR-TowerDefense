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
        Vector3 scale = t.localScale;
        if (scale != startScale)
        {
            t.localScale = startScale;
            string s = (caller == "") ? "" : $". Called by: {caller}";
            Debug.Log($"{name} was the wrong scale. Local scale is {t.localScale}. Setting to start val of {startScale}{s}");
        }
    }
}
