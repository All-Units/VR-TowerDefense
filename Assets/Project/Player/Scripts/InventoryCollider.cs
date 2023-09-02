using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryCollider : MonoBehaviour
{
    public Transform waistPoint;
    public GameObject table;
    public Rigidbody rb;
    public float waitTime = 1f;

    private void Update()
    {
        //if we are moving and not grabbed, reset
        if (isMoving && isGrabbed == false)
            _resetSphere();
    }

    private bool isMoving => (rb.velocity.magnitude > 0.1f || Vector3.Distance(transform.position, waistPoint.position) > 0.1f);

    public void ReturnToWaist()
    {
        if (inInventoryCollider)
        {
            table.SetActive(true);
            StartCoroutine(_waitThenDeactivate());
            isGrabbed = false;
            _resetSphere();
        }
        StartCoroutine(_waitThenReturn());
    }

    IEnumerator _waitThenDeactivate()
    {
        yield return new WaitForSeconds(6f);
        table.SetActive(false);
    }

    private bool isGrabbed = false;
    public void StartGrab()
    {
        isGrabbed = true;
    }

    IEnumerator _waitThenReturn()
    {
        yield return new WaitForSeconds(waitTime);
        isGrabbed = false;
        _resetSphere();
    }

    void _resetSphere()
    {
        transform.parent = waistPoint;
        transform.localPosition = Vector3.zero;
        rb.velocity = Vector3.zero;
    }

    private bool inInventoryCollider = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Inventory") == false) return;
        inInventoryCollider = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Inventory") == false) return;
        inInventoryCollider = false;
    }
}
