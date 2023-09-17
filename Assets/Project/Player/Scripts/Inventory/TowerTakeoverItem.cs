using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TowerTakeoverItem : MonoBehaviour
{
    public XRGrabInteractable table;

    [SerializeField] private Transform mirrorPoint;
    [SerializeField] private float waitTime = 3f;
    [SerializeField] private XRControllerTowerController _controller;
    // Start is called before the first frame update
    void Start()
    {
        table.selectEntered.AddListener(StartGrab);
        table.selectExited.AddListener(EndGrab);
        startRot = transform.localRotation;
    }

    private Quaternion startRot;
    public MeshRenderer _mr;
    // Update is called once per frame
    void Update()
    {
        if (PlayerStateController.instance.state == PlayerState.TOWER_CONTROL)
        {
            if (_mr == null) _mr = GetComponentInChildren<MeshRenderer>();
            _mr.gameObject.SetActive(false);
            //_mr.enabled = false;
            table.enabled = false;
        }
        else
        {
            if (_mr == null) _mr = GetComponentInChildren<MeshRenderer>();
            _mr.gameObject.SetActive(true);
            //_mr.enabled = true;
            table.enabled = true;
            bool hitTime = (Time.time - lastDropTime >= waitTime) || lastDropTime == 0f;
            if (isGrabbed == false && hitTime)
            {
                if (transform.parent != mirrorPoint)
                    transform.parent = mirrorPoint;
                if (transform.localPosition != Vector3.zero)
                    _resetSphere();
            }
        }
    }
    public bool isGrabbed = false;
    private Inventory2 inv;
    public void StartGrab(SelectEnterEventArgs args)
    {
        inv = InventoryManager.invByTor(args.interactorObject);
        isGrabbed = true;
        _controller.inv = inv;

        inv.trigger.action.started += _controller.StartSelection;
        inv.trigger.action.canceled += _controller.EndSelection;
    }
    private float lastDropTime = 0f;
    void EndGrab(SelectExitEventArgs args)
    {
        isGrabbed = false;
        lastDropTime = Time.time;
        _controller.inv = null;
        inv.trigger.action.started -= _controller.StartSelection;
        inv.trigger.action.canceled -= _controller.EndSelection;
        StartCoroutine(_waitThenReturn());
        inv = null;
    }

    #region Drop Logic

    IEnumerator _waitThenReturn()
    {
        yield return new WaitForSeconds(waitTime);
        isGrabbed = false;
        _resetSphere();
    }

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
    void _resetSphere()
    {
        Transform t = transform;
        t.parent = mirrorPoint;
        Vector3 pos = mirrorPoint.position;
        pos.y = InventoryManager.InventoryY;
        t.position = pos;
        rb.velocity = Vector3.zero;
        t.localRotation = startRot;
    }
    
    #endregion
}
