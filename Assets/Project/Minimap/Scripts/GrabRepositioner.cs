using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabRepositioner : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable table;
    [SerializeField] private Rigidbody rb;
    private Transform _zeroPoint;

    private Transform _parent;

    private Transform t;
    // Start is called before the first frame update
    void Start()
    {
        t = transform;
        _parent = t.parent;
        table.selectEntered.AddListener(OnSelectEntered);
        table.selectExited.AddListener(OnSelectExited);
        lastPos = Vector3.negativeInfinity;
        _zeroPoint = Minimap.Zero;
        startPos = _zeroPoint.localPosition;
    }

    private Vector3 lastPos;
    private Vector3 startPos;
    public void Reset()
    {
        _zeroPoint.localPosition = startPos;
        lastPos = Vector3.negativeInfinity;
    }
    private void Update()
    {
        if (grabbed == false)
        {
            if (t.localPosition != Vector3.zero || rb.velocity != Vector3.zero)
                _Zero();
            return;
        }
        Vector3 pos = t.position;
        if (grabbedLastFrame)
        {
            lastPos = pos;
            grabbedLastFrame = false;
        }
        float distance = Vector3.Distance(pos, lastPos);
        if (lastPos.y > -10000f && distance > 0 && distance < 10f)
        {
            _zeroPoint.Translate(pos - lastPos);
            //print($"moved zp {pos - lastPos}, moved to {_zeroPoint.position}");
            Vector3 localPos = _zeroPoint.localPosition;
            var xBounds = Minimap.instance.xBounds;
            var zBounds = Minimap.instance.yBounds;
            if (localPos.x < xBounds.x)
                localPos.x = xBounds.x;
            else if (localPos.x > xBounds.y)
                localPos.x = xBounds.y;
            if (localPos.z < zBounds.x)
                localPos.z = zBounds.x;
            else if (localPos.z > zBounds.y)
                localPos.z = zBounds.y;
            _zeroPoint.localPosition = localPos;

        }

        lastPos = pos;
    }

    private void OnEnable()
    {
        Minimap.CenterPlayer();
        t = transform;
        _parent = t.parent;
        _Zero();
    }

    private bool grabbed = false;
    private bool grabbedLastFrame = false;

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        grabbed = true;
        grabbedLastFrame = true;
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        grabbed = false;
        //lastPos = Vector3.negativeInfinity;
        _Zero();
    }

    void _Zero()
    {
        t.parent = _parent;
        t.localPosition = Vector3.zero;
        t.rotation = Quaternion.identity;
        if (_currentZero == null)
        {
            _currentZero = _ZeroVelocity();
            StartCoroutine(_currentZero);
        }
    }

    private IEnumerator _currentZero = null;
    IEnumerator _ZeroVelocity()
    {
        float startT = Time.time;
        while (Time.time - 0.2f <= startT)
        {
            yield return null;
            rb.velocity = Vector3.zero;
            t.position = _parent.position;
            lastPos = t.position;
            t.rotation = Quaternion.identity;
        }

        _currentZero = null;
    }
}
