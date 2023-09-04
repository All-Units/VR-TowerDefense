using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ArrowSpawner : MonoBehaviour
{
    public GameObject arrow;
    public GameObject notch;

    private XRGrabInteractable _bow;
    private bool _arrowNotched = false;
    private GameObject _currentArrow;

    private void Start()
    {
        _bow = GetComponentInParent<XRGrabInteractable>();
        PullInteraction.PullActionReleased += NotchEmpty;
    }

    private void OnDestroy()
    {
        PullInteraction.PullActionReleased -= NotchEmpty;
    }

    private void Update()
    {
        if (_bow.isSelected && !_arrowNotched)
        {
            StartCoroutine(DelaySpawn());
        }

        if (!_bow.isSelected && _currentArrow)
        {
            Destroy(_currentArrow);
        }
    }

    private void NotchEmpty(float f)
    {
        _arrowNotched = false;
    }

    private IEnumerator DelaySpawn()
    {
        _arrowNotched = true;
        yield return new WaitForSeconds(.1f);
        _currentArrow = Instantiate(arrow, notch.transform);
    }
}