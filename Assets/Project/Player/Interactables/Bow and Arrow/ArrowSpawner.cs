using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ArrowSpawner : MonoBehaviour
{
    public GameObject arrow;
    public GameObject notch;

    [SerializeField] private XRGrabInteractable bow;
    [SerializeField] private PullInteraction pullInteraction;
    private bool _arrowNotched = false;
    private GameObject _currentArrow;

    private void Start()
    {
        // bow = GetComponentInParent<XRGrabInteractable>();
        PullInteraction.PullActionReleased += NotchEmpty;
    }

    private void OnDestroy()
    {
        PullInteraction.PullActionReleased -= NotchEmpty;
    }

    /*private void Update()
    {
        if (_bow.isSelected && !_arrowNotched)
        {
            StartCoroutine(DelaySpawn());
        }

        if (!_bow.isSelected && _currentArrow)
        {
            Destroy(_currentArrow);
        }
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if(bow.isSelected == false || _arrowNotched) return;
        //Debug.Log($"Notching {other.gameObject.name}");

        if (other.GetComponent<Arrow>())
        {
            if(other.TryGetComponent<XRGrabInteractable>(out var objectInteractable) == false || objectInteractable.isSelected == false) return;
            
            //Debug.Log("is arrow!");
            _currentArrow = other.gameObject;
            _arrowNotched = true;
            var interactor = objectInteractable.GetOldestInteractorSelecting();
            objectInteractable.interactionManager.SelectExit(interactor, objectInteractable);
            pullInteraction.interactionManager.SelectEnter(interactor, pullInteraction);
            
            var currentController = interactor.transform.gameObject.GetComponentInParent<ActionBasedController>();
            currentController.SendHapticImpulse(3, 0.5f);
            
            _currentArrow.transform.SetParent(notch.transform);
            _currentArrow.transform.localPosition = Vector3.forward * .05f;
            _currentArrow.transform.localRotation = Quaternion.identity;
        }
    }

    private void NotchEmpty(float f, TowerPlayerWeapon towerPlayerWeapon)
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