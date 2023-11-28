using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScopeController : MonoBehaviour
{
    [SerializeField] private float offset;
    [SerializeField] private float _maxDistance = 100f;
    [SerializeField] private Transform camTransform;

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(transform.position, transform.forward, out var hit, _maxDistance))
        {
            camTransform.position = hit.point + transform.forward * offset;
        }
        else
        {
            camTransform.position = transform.position + (transform.forward * _maxDistance);
        }
    }
}
