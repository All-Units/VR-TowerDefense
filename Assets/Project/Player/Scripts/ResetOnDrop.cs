using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
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
    public Vector3 CurrentPosition;
    RigidbodyConstraints _startConstraints;
    
    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
        if (table == null)
            table = GetComponent<XRBaseInteractable>();

        PlayerStateController.OnStateChange += _OnStateChange;
    }

    private void _OnStateChange(PlayerState oldState, PlayerState newState)
    {
        var tower = PlayerStateController.CurrentTower;
        //If we're switching to idle, always hide everything
        print($"{playerItem} switching from {oldState} to {newState}");
        if (newState == PlayerState.IDLE)
        {
            print("Idle, hiding");
            _HideItem();
        }
        else if (tower.dto is ProjectileTower_SO dto)
        {
            print($"Tower, was ProjectileTower, was self? {dto.playerItem_SO != playerItem}");
            //If the new tower is NOT our item type
            if (dto.playerItem_SO != playerItem)
            {
                _HideItem();
            }
        }
    }
    void _HideItem()
    {
        rb.constraints = RigidbodyConstraints.None;
        t.position = new Vector3(0f, -1000f, 0f);
        rb.constraints = RigidbodyConstraints.FreezeAll;
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
        _startConstraints = rb.constraints;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        if (Vector3.Distance(CurrentPosition, pos) >= 0.8f)
        {
            //print($"Jumping! From {CurrentPosition} to {transform.position}. Distance: {Vector3.Distance(CurrentPosition, pos)}");
        }
        CurrentPosition = transform.position;
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
        rb.constraints = _startConstraints;
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
        //print("Cannon dropped");
        yield return new WaitForSeconds(resetTime);
        //Do nothing if there is no tower
        var playerControllableTower = PlayerStateController.CurrentTower;
        if (playerControllableTower == null)
        {
            print($"Was no tower, NOT resetting");
            yield break;
        }
        if (playerControllableTower.dto is ProjectileTower_SO ptso && ptso.playerItem_SO != playerItem)
        {
            _HideItem();
            yield break;
        }
        //Transform currentTowerTransform = playerControllableTower.GetPlayerControlPoint();
        Transform cameraTransform = InventoryManager.instance.playerCameraTransform;
        Vector3 pos = cameraTransform.position;
        //pos.y += 1.5f;//offset.y;
        Vector3 forward = cameraTransform.forward; forward.y = 0f;
        forward = forward.normalized;
        pos += (forward * 0.6f);
        t.position = pos;
        rb.velocity = Vector3.zero;
        //Constrain it
        rb.constraints = RigidbodyConstraints.FreezeAll;
        yield return null;
        //PEMDAS
        rb.velocity = Vector3.zero;

        t.position = pos;
        //Debug.Log($"Reset position to {pos}, actual position: {t.position}. at FC {Time.frameCount}", gameObject);
        t.rotation = startRot;
        _currentResetter = null;
        rb.useGravity = false;
        
    }
}
