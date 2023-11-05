using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ResetOnDrop : MonoBehaviour
{
    [SerializeField] private XRBaseInteractable table;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float resetTime = 2f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, 0.5f);

    private Quaternion startRot;

    private Transform t;
    
    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
        if (table == null)
            table = GetComponent<XRBaseInteractable>();
    }

    private void OnEnable()
    {
        InventoryManager.OnItemsHidden.AddListener(_OnAllItemsHidden);
    }

    private void OnDisable()
    {
        InventoryManager.OnItemsHidden.RemoveListener(_OnAllItemsHidden);
    }

    void _OnAllItemsHidden()
    {
        if (_currentResetter != null)
        {
            StopCoroutine(_currentResetter);
            _currentResetter = null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        t = transform;
        table.selectEntered.AddListener(OnSelectEntered);
        table.selectExited.AddListener(OnSelectExited);
        startRot = t.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        //Don't update pos if held
        if (table.interactorsSelecting.Count != 0)
            return;
        if (rb.velocity != Vector3.zero && _currentResetter == null)
        {
            _currentResetter = _ResetPosition();
            StartCoroutine(_currentResetter);
        }
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        //print($"{gameObject.name} grabbed");
        if (_currentResetter != null)
        {
            StopCoroutine(_currentResetter);
            _currentResetter = null;
        }
        rb.useGravity = true;
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        rb.useGravity = true;
        //print($"{gameObject} dropped");
        if (_currentResetter == null && gameObject.activeInHierarchy)
        {
            _currentResetter = _ResetPosition();
            StartCoroutine(_currentResetter);
        }
    }

    private IEnumerator _currentResetter = null;
    IEnumerator _ResetPosition()
    {
        yield return new WaitForSeconds(resetTime);
        //Do nothing if there is no tower
        if (PlayerStateController.CurrentTower == null)
        {
            //print($"Was no tower, NOT resetting");
            yield break;
        }
        Transform tower = PlayerStateController.CurrentTower.transform;
        Vector3 pos = tower.position;
        pos.y += offset.y;
        pos += (tower.forward * offset.z);
        rb.velocity = Vector3.zero;
        t.position = pos;
        t.rotation = startRot;
        _currentResetter = null;
        //print($"Reset position to {pos}");
        rb.useGravity = false;
        
        //Constrain it
        rb.constraints = RigidbodyConstraints.FreezeAll;
        yield return null;
        t.rotation = startRot;
        yield return null;
        t.position = pos;
        rb.constraints = RigidbodyConstraints.None;
    }
}
