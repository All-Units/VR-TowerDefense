using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _teleTableItem : MonoBehaviour
{
    public Transform teleporter;

    
    private void OnEnable()
    {
        teleporter.gameObject.SetActive(true);
        teleporter.rotation = transform.rotation;
        teleporter.position = transform.position;
    }

    private void OnDisable()
    {
        teleporter.gameObject.SetActive(false);
    }
}
