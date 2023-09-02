using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InventoryItem : MonoBehaviour
{
    public GameObject inventoryParent;
    public XRGrabInteractable grabber;

    [SerializeField] private float waitBeforeResetTime = 4f;
    
    private Vector3 startPos;
    private Quaternion startRot;

    private Rigidbody rb
    {
        get
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();
            return _rb;
        }
    }

    private Rigidbody _rb;


    // Start is called before the first frame update
    void Awake()
    {
        grabber.selectEntered.AddListener(OnPickup);
        grabber.selectExited.AddListener(OnDrop);
        startPos = transform.localPosition;
        startRot = transform.localRotation;
    }

    private void Update()
    {
        
        if (isHeld == false)
        {
            Vector3 pos = transform.localPosition;
            if (Vector3.Distance(pos, startPos) > 0.01f)
                Reset();
        }
    }

    private void OnEnable()
    {
        Reset();
    }

    private bool isHeld = false;
    public void OnPickup(SelectEnterEventArgs args)
    {
        isHeld = true;
        if (currentWaiting != null)
            StopCoroutine(currentWaiting);
        transform.parent = null;
        inventoryParent.SetActive(false);
    }

    

    public void OnDrop(SelectExitEventArgs args)
    {
        
        currentWaiting = waitThenReset();
        StartCoroutine(waitThenReset());
    }

    private IEnumerator currentWaiting = null;
    IEnumerator waitThenReset()
    {
        yield return new WaitForSeconds(waitBeforeResetTime);
        currentWaiting = null;
        isHeld = false;
        Reset();
    }

    public void Reset()
    {
        transform.parent = inventoryParent.transform;
        transform.localPosition = startPos;
        transform.localRotation = startRot;
        rb.velocity = Vector3.zero;
    }
}
