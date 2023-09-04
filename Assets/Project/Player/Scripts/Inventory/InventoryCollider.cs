using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InventoryCollider : MonoBehaviour
{
    public Transform waistPoint;
    public GameObject table;
    public ParticleSystem starsParticles;
    public Rigidbody rb;
    public float waitTime = 1f;

    private void Start()
    {
        starsParticles.gameObject.SetActive(false);
        table.SetActive(false);
    }

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
            OpenInventory();
            return;
        }
        StartCoroutine(_waitThenReturn());
    }

    IEnumerator _waitThenDeactivate()
    {
        positionTable();
        yield return new WaitForSeconds(0.8f);
        table.SetActive(true);
        yield return new WaitForSeconds(9f);
        table.SetActive(false);
        currentOpener = null;
    }

    private IEnumerator currentOpener = null;
    void OpenInventory()
    {
        GameObject particles = Instantiate(starsParticles.gameObject, null);
        particles.transform.position = starsParticles.transform.position;
        particles.SetActive(true);
        if (currentOpener != null)
        {
            StopCoroutine(currentOpener);
        }
        
        
        currentOpener = _waitThenDeactivate();
        StartCoroutine(currentOpener);
        isGrabbed = false;
        _resetSphere();
    }

    private LocomotionSystem _ls;
    void positionTable()
    {
        Vector3 pos = transform.position;
        /*
        if (_ls == null)
            _ls = GetComponentInParent<LocomotionSystem>();
        Vector3 dir = _ls.transform.position;
        dir -= pos;
        dir.y = 0f;
        
        float degrees = Mathf.Atan2(dir.x, dir.z) * (180f / Mathf.PI);
        Vector3 euler = table.transform.localEulerAngles;
        euler.y = degrees;*/
        table.transform.position = pos;
        
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
