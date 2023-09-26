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

    [SerializeField] private float waitTime = 4f;
    // Start is called before the first frame update
    void Start()
    {
        table.selectEntered.AddListener(StartSelected);
        table.selectExited.AddListener(EndSelected);
    }

    // Update is called once per frame
    void Update()
    {
        if (enteredTip)
        {
            transform.position = new Vector3(0f, -1000f, 0f);
            return;
        }
        if (_isHeld == false && (Time.time - lastDropTime >= waitTime || lastDropTime == 0f))
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
        enteredTip = false;
    }

    private float lastDropTime = 0f;
    void EndSelected(SelectExitEventArgs args)
    {
        _isHeld = false;
        enteredTip = false;
        lastDropTime = Time.time;
        icon.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isHeld == false) return;
        if (other.CompareTag("TowerTip"))
        {
            enteredTip = true;
            _selector.SelectTower(icon.towerSO);
            _selector.CloseInventory();
            icon.gameObject.SetActive(false);
        }
    }
}
