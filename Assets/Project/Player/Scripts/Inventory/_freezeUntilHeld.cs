using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class _freezeUntilHeld : MonoBehaviour
{
    public Transform mirrorPoint;

    public XRGrabInteractable table;
    public Material green;

    public TowerSelector _selector;

    [SerializeField] private TowerIcon icon;
    // Start is called before the first frame update
    void Start()
    {
        table.selectEntered.AddListener(StartSelected);
        table.selectExited.AddListener(EndSelected);
    }

    // Update is called once per frame
    void Update()
    {
        if (enteredTip) return;
        if (_isHeld == false)
        {
            if (mirrorPoint.gameObject.activeInHierarchy)
            {
                transform.position = mirrorPoint.position;
                transform.rotation = mirrorPoint.rotation;
            }
            else
            {
                transform.position = new Vector3(0f, -1000f, 0f);
            }
            
        }
    }

    private bool _isHeld = false;
    public bool enteredTip = false;
    void StartSelected(SelectEnterEventArgs args)
    {
        _isHeld = true;
    }

    void EndSelected(SelectExitEventArgs args)
    {
        _isHeld = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isHeld == false) return;
        if (other.CompareTag("TowerTip"))
        {
            _isHeld = false;
            _selector.SelectTower(icon.towerSO);
        }
    }
}
