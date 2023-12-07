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
    private Vector3 startScale;
    protected virtual void Awake()
    {
        grabber.selectEntered.AddListener(OnPickup);
        grabber.selectExited.AddListener(OnDrop);
        startPos = transform.localPosition;
        startRot = transform.localRotation;
        startScale = transform.localScale;
    }

    private void Update()
    {
        
        if (isHeld == false && currentWaiting == null)
        {
            Vector3 pos = transform.localPosition;
            if (Vector3.Distance(pos, startPos) > 0f)
                Reset();
        }
    }

    private void OnEnable()
    {
        Reset();
        Vector3 scale = transform.localScale;
        if (scale != startScale)
        {
            Debug.Log($"{name} was the wrong scale, start scale was {startScale}, currently {scale}. Local scale is {transform.localScale}");
        }
    }

    private bool isHeld = false;
    public virtual void OnPickup(SelectEnterEventArgs args)
    {
        isHeld = true;
        if (currentWaiting != null)
            StopCoroutine(currentWaiting);
        transform.parent = null;
        //inventoryParent.SetActive(false);
    }

    

    public virtual void OnDrop(SelectExitEventArgs args)
    {
        isHeld = false;
        if (gameObject.activeInHierarchy == false) return;
        currentWaiting = waitThenReset();
        StartCoroutine(waitThenReset());
    }

    private IEnumerator currentWaiting = null;
    IEnumerator waitThenReset()
    {
        yield return new WaitForSeconds(waitBeforeResetTime);
        
        
        currentWaiting = null;
        if (isHeld) yield break;
        Reset();
    }

    public void Reset()
    {
        Transform t = transform;
        t.parent = inventoryParent.transform;
        t.localPosition = startPos;
        t.localRotation = startRot;
        rb.velocity = Vector3.zero;
        if (transform.localScale != startScale)
        {
            //print($"Manually rescaling back to {startScale} from {t.localScale}");
            transform.localScale = startScale;
        }
    }
}
