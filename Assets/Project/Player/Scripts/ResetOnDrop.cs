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
    const float resetTime = 3f;
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
        table.selectEntered.AddListener(OnItemGrabbed);
        table.selectExited.AddListener(OnItemDropped);
        if (table.interactorsSelecting.Count > 0)
        {
            _SetAllTriggers();

        }
        PlayerStateController.OnStateChange += _OnStateChange;
    }

    private void _OnStateChange(PlayerState oldState, PlayerState newState)
    {
        var tower = PlayerStateController.CurrentTower;
        //If we're switching to idle, always hide everything
        //print($"{playerItem} switching from {oldState} to {newState}");
        if (newState == PlayerState.IDLE)
        {
            //print("Idle, hiding");
            _HideItem();
        }
        else if (tower.dto is ProjectileTower_SO dto)
        {
            //print($"Tower, was ProjectileTower, was self? {dto.playerItem_SO != playerItem}");
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
    void _SetTriggers()
    {
        if (_wasTriggerBefore.Count != 0) return;
        foreach (var col in GetComponentsInChildren<Collider>(true))
        {
            _wasTriggerBefore.Add(col, col.isTrigger);
        }
    }
    Dictionary<Collider, bool> _wasTriggerBefore = new Dictionary<Collider, bool>();

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
            _currentResetter = _ResetRoutine();
            StartCoroutine(_currentResetter);
        }
    }

    void OnItemGrabbed(SelectEnterEventArgs args)
    {
        //print($"{gameObject.name} grabbed");
        if (_currentResetter != null)
        {
            StopCoroutine(_currentResetter);
            _currentResetter = null;
        }
        rb.useGravity = true;
        rb.constraints = _startConstraints;
        
        //Make all colliders triggers while we're holding it
        _SetAllTriggers();
    }
    void _SetAllTriggers()
    {
        _SetTriggers();
        foreach (var cols in _wasTriggerBefore)
        {
            cols.Key.isTrigger = true;
        }
    }
    
    void OnItemDropped(SelectExitEventArgs args)
    {
        //Do nothing if we're still holding the object in our other hand!
        if (table.interactorsSelecting.Count > 0) return;
        
        rb.useGravity = true;
        if (_currentResetter == null && gameObject.activeInHierarchy)
        {
            _currentResetter = _ResetRoutine();
            StartCoroutine(_currentResetter);
        }
        _SetTriggers();
        //Set colliders back to their base trigger status
        foreach (var col in _wasTriggerBefore)
        {
            col.Key.isTrigger = col.Value;
        }
    }

    private IEnumerator _currentResetter = null;
    IEnumerator _ResetRoutine()
    {
        //print("Cannon dropped");
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
