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
    [SerializeField] private Vector3 offset = new Vector3(0f, 4f, 0.5f);

    public TowerTakeoverObject playerItem;

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
        var playerControllableTower = PlayerStateController.CurrentTower;
        if (playerControllableTower == null)
        {
            //print($"Was no tower, NOT resetting");
            yield break;
        }
        if (playerControllableTower.dto is ProjectileTower_SO ptso && ptso.playerItem_SO != playerItem)
        {
            Debug.Log($"Was in the wrong tower, not resetting! {gameObject.name}", gameObject);
            t.position = new Vector3(0f, -1000f, 0f);
            rb.useGravity = false;
            yield break;
        }
        Transform currentTowerTransform = playerControllableTower.GetPlayerControlPoint();
        Vector3 pos = currentTowerTransform.position;
        pos.y += 1.5f;//offset.y;
        pos += (currentTowerTransform.forward * offset.z);
        rb.velocity = Vector3.zero;
        //Constrain it
        rb.constraints = RigidbodyConstraints.FreezeAll;
        yield return null;
        //PEMDAS
        rb.velocity = Vector3.zero;

        t.position = pos;
        t.rotation = startRot;
        _currentResetter = null;
        rb.useGravity = false;
        
        
        yield return null;
        t.rotation = startRot;
        yield return null;
        t.position = pos;
        rb.constraints = RigidbodyConstraints.None;
    }
}
