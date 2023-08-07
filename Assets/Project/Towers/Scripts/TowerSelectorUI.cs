using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class TowerSelectorUI : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private DynamicMoveProvider playerMover;
    [SerializeField] private InputActionReference navigateControl;
    [SerializeField] private InputActionReference grip;
    [Header("References")]
    [SerializeField] private GameObject selectPanel;
    [SerializeField] private RectTransform pointerTransform;
    [SerializeField] private GameObject dividingLine;
    [SerializeField] private GameObject towerIconPrefab;
    [SerializeField] private Transform towerIconsParent;
    [SerializeField] private List<Sprite> towerPrefabs = new List<Sprite>();
    [SerializeField] private float angleOffset = -45f;

    private void Awake()
    {
        FillWheel();
        selectPanel.SetActive(false);
    }

    private void OnEnable()
    {
        grip.action.started += LeftHandGrip;
        grip.action.canceled += LeftHandGrip;
        
    }

    private void OnDisable()
    {
        grip.action.started -= LeftHandGrip;
        grip.action.canceled -= LeftHandGrip;
        OnCloseSelector();
    }
    /// <summary>
    /// Logic when the player changes grip in the left hand
    /// </summary>
    /// <param name="obj"></param>
    void LeftHandGrip(InputAction.CallbackContext obj)
    {
        if (obj.phase == InputActionPhase.Started)
        {
            OnOpenSelector();
            
        }
        else if (obj.phase == InputActionPhase.Canceled)
        {
            OnCloseSelector();
        }
    }
    /// <summary>
    /// Logic when opening the tower select panel
    /// </summary>
    void OnOpenSelector()
    {
        navigateControl.action.performed += OnLook;
        selectPanel.SetActive(true);
        //Disable player movement
        playerMover.CanMove = false;
    }
    
    void OnCloseSelector()
    {
        navigateControl.action.performed -= OnLook;
        selectPanel.SetActive(false);
        //Reenable player movement
        playerMover.CanMove = true;
    }
    public void OnLook(InputAction.CallbackContext obj)
    {
        PointArrow(obj.ReadValue<Vector2>());
    }

    public float deg;
    void PointArrow(Vector2 dir)
    {
        float degrees = Mathf.Atan2(dir.y, dir.x) * (180f / Mathf.PI);
        deg = degrees;
        degrees += angleOffset;
        pointerTransform.localEulerAngles = new Vector3(0f, 0f, degrees);
        
    }

    void FillWheel()
    {
        for (int i = 0; i < towerPrefabs.Count; i++)
        {
            

            GameObject ico = Instantiate(towerIconPrefab, towerIconsParent);
            float arc = (360f / towerPrefabs.Count);
            float degrees = arc * i + (arc / 2f);

            ico.transform.localEulerAngles = new Vector3(0f, 0f, -1f * degrees);
            Image image = ico.GetComponentInChildren<Image>();
            image.sprite = towerPrefabs[i];
            image.transform.localEulerAngles = new Vector3(0f, 0f, degrees);

            if (i != 0)
            {
                GameObject line = Instantiate(dividingLine, dividingLine.transform.parent);
                line.transform.localEulerAngles = new Vector3(0f, 0f, arc * i);
            }
        }
    }
}
