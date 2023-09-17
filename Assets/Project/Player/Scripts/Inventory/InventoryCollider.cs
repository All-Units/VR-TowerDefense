using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

public class InventoryCollider : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable table;
    public Transform waistPoint;
    public GameObject tableModel;
    public ParticleSystem starsParticles;
    [SerializeField] private Transform starPoint;
    public Rigidbody rb;
    public float waitTime = 1f;

    private Quaternion startRot;
    private void Start()
    {
        starsParticles.gameObject.SetActive(false);
        tableModel.SetActive(false);
        startRot = transform.localRotation;
        table.activated.AddListener(TriggerPulled);
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
            //OpenInventory();
            //return;
        }
        StartCoroutine(_waitThenReturn());
    }

    IEnumerator _waitThenDeactivate()
    {
        positionTable();
        yield return new WaitForSeconds(0.8f);
        tableModel.SetActive(true);
        yield return new WaitForSeconds(9f);
        tableModel.SetActive(false);
        currentOpener = null;
    }

    public void TriggerPulled(ActivateEventArgs args)
    {
        OpenInventory();
    }
    private IEnumerator currentOpener = null;
    void OpenInventory()
    {
        GameObject particles = Instantiate(starsParticles.gameObject, null);
        particles.transform.position = starPoint.position;
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
        tableModel.transform.position = pos;
        
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
        Transform t = transform;
        t.parent = waistPoint;
        Vector3 pos = waistPoint.position;
        pos.y = InventoryManager.InventoryY;
        rb.velocity = Vector3.zero;
        t.localRotation = startRot;
        t.position = pos;
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
